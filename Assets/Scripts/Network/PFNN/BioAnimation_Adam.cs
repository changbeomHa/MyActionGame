using UnityEngine;
using System;
using System.Collections.Generic;
using DeepLearning;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SIGGRAPH_2018
{
    [RequireComponent(typeof(Actor))]
    public class BioAnimation_Adam : MonoBehaviour
    {
        public bool Inspect = false;

        public bool ShowTrajectory = true;
        public bool ShowVelocities = true;

        public float TargetGain = 0.25f;
        public float TargetDecay = 0.05f;
        public bool TrajectoryControl = true;
        public float TrajectoryCorrection = 1f;

        public int TrajectoryDimIn = 6;
        public int TrajectoryDimOut = 6;
        public int JointDimIn = 12;
        public int JointDimOut = 12;

        public Controller Controller;

        private Actor Actor;
        private PFNN NN;
        private Trajectory Trajectory;

        private Vector3 TargetDirection;
        private Vector3 TargetVelocity;

        //State
        private Vector3[] Positions = new Vector3[0];
        private Vector3[] Forwards = new Vector3[0];
        private Vector3[] Ups = new Vector3[0];
        private Vector3[] Velocities = new Vector3[0];

        //Trajectory for 60 Hz framerate
        private int Framerate = 30; // default value is 60
        private const int Points = 111;
        private const int PointSamples = 12;
        private const int PastPoints = 60;
        private const int FuturePoints = 50;
        private const int RootPointIndex = 60;
        private const int PointDensity = 10;

        private Socket socket;

        private string socket_input = "";
        private List<int> parents = new List<int>(new int[] {
                                                                0,
                                                                0, 1, 2, 3 , 4, 5, 6,
                                                                4, 8, 9, 10, 11,
                                                                4, 13, 14, 15, 16,
                                                                0, 18, 19, 20, 21,
                                                                0, 23, 24, 25, 26
                                                            });
        private List<int> skeletons = new List<int>(new int[] {
                                                                0,
                                                                23, 24, 25, 26,
                                                                18, 19, 20, 21,
                                                                2, 4, 5, 6,
                                                                14, 15, 16, 17,
                                                                9, 10, 11, 12
                                                            });
        private List<float> distances = new List<float>(new float[28]);
        private List<Vector3> UpVector = new List<Vector3>(new Vector3[28]);
        private List<Vector3> ForwardVector = new List<Vector3>(new Vector3[28]);

        void Reset()
        {
            Controller = new Controller();
        }

        void Awake()
        {
            Actor = GetComponent<Actor>();
            NN = GetComponent<PFNN>();
            TargetDirection = new Vector3(transform.forward.x, 0f, transform.forward.z);
            TargetVelocity = Vector3.zero;
            Positions = new Vector3[Actor.Bones.Length];
            Forwards = new Vector3[Actor.Bones.Length];
            Ups = new Vector3[Actor.Bones.Length];
            Velocities = new Vector3[Actor.Bones.Length];
            Trajectory = new Trajectory(Points, Controller.GetNames(), transform.position, TargetDirection);
            Trajectory.Postprocess(); // post process for foot ik with ground

            for (int i = 0; i < Actor.Bones.Length; i++)
            {
                if (i == 0)
                {
                    distances[i] = Vector3.Distance(Trajectory.Points[RootPointIndex].GetPosition(), Actor.Bones[i].Transform.position);
                }
                else
                {
                    distances[i] = Vector3.Distance(Actor.Bones[parents[i]].Transform.position, Actor.Bones[i].Transform.position);
                }
                UpVector[i] = Actor.Bones[i].Transform.rotation.GetUp();
                ForwardVector[i] = Actor.Bones[i].Transform.rotation.GetForward();
            }

            if (Controller.Styles.Length > 0)
            {
                for (int i = 0; i < Trajectory.Points.Length; i++)
                {
                    Trajectory.Points[i].Styles[0] = 1f;
                }
            }
            for (int i = 0; i < Actor.Bones.Length; i++)
            {
                Positions[i] = Actor.Bones[i].Transform.position;
                Forwards[i] = Actor.Bones[i].Transform.forward;
                Ups[i] = Actor.Bones[i].Transform.up;
                Velocities[i] = Vector3.zero;
            }

            if (NN.Parameters == null)
            {
                Debug.Log("No parameters saved.");
                return;
            }
            NN.LoadParameters();
        }

        void Start()
        {
            Utility.SetFPS(60);
        }

        void LateUpdate()
        {
            if (NN.Parameters == null)
            {
                return;
            }

            if (TrajectoryControl)
            {
                PredictTrajectory();
            }

            if (NN.Parameters != null)
            {
                Animate();
            }

            transform.position = Trajectory.Points[RootPointIndex].GetPosition();
        }

        private void PredictTrajectory()
        {
            //Calculate Bias
            float bias = PoolBias();

            //Determine Control
            float turn = Controller.QueryTurn();
            Vector3 move = Controller.QueryMove();
            bool control = turn != 0f || move != Vector3.zero;

            //Update Target Direction / Velocity / Correction
            TargetDirection = Vector3.Lerp(TargetDirection, Quaternion.AngleAxis(turn * 60f, Vector3.up) * Trajectory.Points[RootPointIndex].GetDirection(), control ? TargetGain : TargetDecay);
            TargetVelocity = Vector3.Lerp(TargetVelocity, bias * (Quaternion.LookRotation(TargetDirection, Vector3.up) * move).normalized, control ? TargetGain : TargetDecay);
            TrajectoryCorrection = Utility.Interpolate(TrajectoryCorrection, Mathf.Max(move.normalized.magnitude, Mathf.Abs(turn)), control ? TargetGain : TargetDecay);

            //Predict Future Trajectory
            Vector3[] trajectory_positions_blend = new Vector3[Trajectory.Points.Length];
            trajectory_positions_blend[RootPointIndex] = Trajectory.Points[RootPointIndex].GetTransformation().GetPosition();
            for (int i = RootPointIndex + 1; i < Trajectory.Points.Length; i++)
            {
                float bias_pos = 0.75f;
                float bias_dir = 1.25f;
                float bias_vel = 1.50f;
                float weight = (float)(i - RootPointIndex) / (float)FuturePoints; //w between 1/FuturePoints and 1
                float scale_pos = 1.0f - Mathf.Pow(1.0f - weight, bias_pos);
                float scale_dir = 1.0f - Mathf.Pow(1.0f - weight, bias_dir);
                float scale_vel = 1.0f - Mathf.Pow(1.0f - weight, bias_vel);

                float scale = 1f / (Trajectory.Points.Length - (RootPointIndex + 1f));

                trajectory_positions_blend[i] = trajectory_positions_blend[i - 1] +
                   Vector3.Lerp(
                   Trajectory.Points[i].GetPosition() - Trajectory.Points[i - 1].GetPosition(),
                   scale * TargetVelocity,
                   scale_pos
                   );

                Trajectory.Points[i].SetDirection(Vector3.Lerp(Trajectory.Points[i].GetDirection(), TargetDirection, scale_dir));
                Trajectory.Points[i].SetVelocity(Vector3.Lerp(Trajectory.Points[i].GetVelocity(), TargetVelocity, scale_vel));
            }
            for (int i = RootPointIndex + 1; i < Trajectory.Points.Length; i++)
            {
                Trajectory.Points[i].SetPosition(trajectory_positions_blend[i]);
            }

            float[] style = Controller.GetStyle();
            if (style[2] == 0f)
            {
                style[1] = Mathf.Max(style[1], Mathf.Clamp(Trajectory.Points[RootPointIndex].GetVelocity().magnitude, 0f, 1f));
            }
            for (int i = RootPointIndex; i < Trajectory.Points.Length; i++)
            {
                float weight = (float)(i - RootPointIndex) / (float)FuturePoints; //w between 0 and 1
                for (int j = 0; j < Trajectory.Points[i].Styles.Length; j++)
                {
                    Trajectory.Points[i].Styles[j] = Utility.Interpolate(Trajectory.Points[i].Styles[j], style[j], Utility.Normalise(weight, 0f, 1f, Controller.Styles[j].Transition, 1f));
                }
                Utility.Normalise(ref Trajectory.Points[i].Styles);
                Trajectory.Points[i].SetSpeed(Utility.Interpolate(Trajectory.Points[i].GetSpeed(), TargetVelocity.magnitude, control ? TargetGain : TargetDecay));
            }

            // post process for foot ik with ground
            for (int i = RootPointIndex + PointDensity; i < Trajectory.Points.Length; i += PointDensity)
            {
                Trajectory.Points[i].Postprocess();
            }
        }

        private String Stylied(Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel)
        {
            string temp = "";
            temp += string.Format("{0:0.######}", pos.x) + ",";
            temp += string.Format("{0:0.######}", pos.y) + ",";
            temp += string.Format("{0:0.######}", pos.z) + ",";
            temp += string.Format("{0:0.######}", forward.x) + ",";
            temp += string.Format("{0:0.######}", forward.y) + ",";
            temp += string.Format("{0:0.######}", forward.z) + ",";
            temp += string.Format("{0:0.######}", up.x) + ",";
            temp += string.Format("{0:0.######}", up.y) + ",";
            temp += string.Format("{0:0.######}", up.z) + ",";
            temp += string.Format("{0:0.######}", vel.x) + ",";
            temp += string.Format("{0:0.######}", vel.y) + ",";
            temp += string.Format("{0:0.######}", vel.z) + ",";

            return temp;
        }

        private void Animate()
        {
            //Calculate Root
            Matrix4x4 currentRoot = Trajectory.Points[RootPointIndex].GetTransformation();
            // currentRoot[1,3] = 0f; //For flat terrain

            int start = 0;
            //Input Trajectory Positions / Directions / Velocities / Styles
            for (int i = 0; i < PointSamples; i++)
            {
                Vector3 pos = GetSample(i).GetPosition().GetRelativePositionTo(currentRoot);
                Vector3 dir = GetSample(i).GetDirection().GetRelativeDirectionTo(currentRoot);
                Vector3 vel = GetSample(i).GetVelocity().GetRelativeDirectionTo(currentRoot);
                float speed = GetSample(i).GetSpeed();
                NN.SetInput(start + i * TrajectoryDimIn + 0, pos.x);
                NN.SetInput(start + i * TrajectoryDimIn + 1, pos.z);
                NN.SetInput(start + i * TrajectoryDimIn + 2, dir.x);
                NN.SetInput(start + i * TrajectoryDimIn + 3, dir.z);
                NN.SetInput(start + i * TrajectoryDimIn + 4, vel.x);
                NN.SetInput(start + i * TrajectoryDimIn + 5, vel.z);
                NN.SetInput(start + i * TrajectoryDimIn + 6, speed);
                for (int j = 0; j < Controller.Styles.Length; j++)
                {
                    NN.SetInput(start + i * TrajectoryDimIn + (TrajectoryDimIn - Controller.Styles.Length) + j, GetSample(i).Styles[j]);
                }
            }
            start += TrajectoryDimIn * PointSamples;

            Matrix4x4 previousRoot = Trajectory.Points[RootPointIndex - 1].GetTransformation();
            // previousRoot[1,3] = 0f; //For flat terrain

            // animation interporation 
            if (ControlManager.isBasicControl)
            {
                for (int i = 0; i < Actor.Bones.Length; i++)
                {
                    Positions[i] = Actor.Bones[i].Transform.position;
                    Forwards[i] = Actor.Bones[i].Transform.forward.normalized;
                    Ups[i] = Actor.Bones[i].Transform.up.normalized;
                }
            }
            //Input Previous Bone Positions / Velocities
            for (int i = 0; i < Actor.Bones.Length; i++)
            {
                Vector3 pos = Positions[i].GetRelativePositionTo(previousRoot);
                Vector3 forward = Forwards[i].GetRelativeDirectionTo(previousRoot);
                Vector3 up = Ups[i].GetRelativeDirectionTo(previousRoot);
                Vector3 vel = Velocities[i].GetRelativeDirectionTo(previousRoot);
                NN.SetInput(start + i * JointDimIn + 0, pos.x);
                NN.SetInput(start + i * JointDimIn + 1, pos.y);
                NN.SetInput(start + i * JointDimIn + 2, pos.z);
                NN.SetInput(start + i * JointDimIn + 3, forward.x);
                NN.SetInput(start + i * JointDimIn + 4, forward.y);
                NN.SetInput(start + i * JointDimIn + 5, forward.z);
                NN.SetInput(start + i * JointDimIn + 6, up.x);
                NN.SetInput(start + i * JointDimIn + 7, up.y);
                NN.SetInput(start + i * JointDimIn + 8, up.z);
                NN.SetInput(start + i * JointDimIn + 9, vel.x);
                NN.SetInput(start + i * JointDimIn + 10, vel.y);
                NN.SetInput(start + i * JointDimIn + 11, vel.z);
            }
            start += JointDimIn * Actor.Bones.Length;

            //Predict
            float rest = Mathf.Pow(1.0f - Trajectory.Points[RootPointIndex].Styles[0], 0.25f);
            ((PFNN)NN).SetDamping(1f - (rest * 0.9f + 0.1f));
            NN.Predict();

            // Player Attacking
            Vector3 AttackRotation = ControlManager.isAttack ? ControlManager.targetVector : new Vector3(0, 0, 0);

            //Update Past Trajectory
            for (int i = 0; i < RootPointIndex; i++)
            {
                Trajectory.Points[i].SetPosition(Trajectory.Points[i + 1].GetPosition());
                Trajectory.Points[i].SetDirection(Trajectory.Points[i + 1].GetDirection());
                Trajectory.Points[i].SetVelocity(Trajectory.Points[i + 1].GetVelocity());
                Trajectory.Points[i].SetSpeed(Trajectory.Points[i + 1].GetSpeed());
                for (int j = 0; j < Trajectory.Points[i].Styles.Length; j++)
                {
                    Trajectory.Points[i].Styles[j] = Trajectory.Points[i + 1].Styles[j];
                }
            }

            //Update Root
            Vector3 translationalOffset = Vector3.zero;
            float rotationalOffset = 0f;
            Vector3 rootMotion = new Vector3(NN.GetOutput(TrajectoryDimOut * 6 + JointDimOut * Actor.Bones.Length + 0), NN.GetOutput(TrajectoryDimOut * 6 + JointDimOut * Actor.Bones.Length + 1), NN.GetOutput(TrajectoryDimOut * 6 + JointDimOut * Actor.Bones.Length + 2));
            rootMotion /= Framerate;
            translationalOffset = rest * new Vector3(rootMotion.x, 0f, rootMotion.z);
            rotationalOffset = rest * rootMotion.y;

            Trajectory.Points[RootPointIndex].SetPosition(translationalOffset.GetRelativePositionFrom(currentRoot));
            Trajectory.Points[RootPointIndex].SetDirection(Quaternion.AngleAxis(rotationalOffset, Vector3.up) * Trajectory.Points[RootPointIndex].GetDirection() + AttackRotation);
            Trajectory.Points[RootPointIndex].SetVelocity(translationalOffset.GetRelativeDirectionFrom(currentRoot) * Framerate);
            Trajectory.Points[RootPointIndex].Postprocess(); // post process for foot ik with ground
            Matrix4x4 nextRoot = Trajectory.Points[RootPointIndex].GetTransformation();
            if (ControlManager.isAttack)
            {
                Trajectory.Points[RootPointIndex].SetVelocity(new Vector3(0, 0, 0));
            }

            // nextRoot[1,3] = 0f; //For flat terrain

            //Update Future Trajectory
            for (int i = RootPointIndex + 1; i < Trajectory.Points.Length; i++)
            {
                Trajectory.Points[i].SetPosition(Trajectory.Points[i].GetPosition() + rest * translationalOffset.GetRelativeDirectionFrom(nextRoot));
                Trajectory.Points[i].SetDirection(Quaternion.AngleAxis(rotationalOffset, Vector3.up) * Trajectory.Points[i].GetDirection() + AttackRotation);
                Trajectory.Points[i].SetVelocity(Trajectory.Points[i].GetVelocity() + translationalOffset.GetRelativeDirectionFrom(nextRoot) * Framerate);
            }

            // post process for foot ik with ground
            for (int i = RootPointIndex; i < Trajectory.Points.Length; i += PointDensity)
            {
                Trajectory.Points[i].Postprocess();
            }

            start = 0;
            for (int i = RootPointIndex + 1; i < Trajectory.Points.Length; i++)
            {
                //ROOT   1      2      3      4      5
                //.x....x.......x.......x.......x.......x
                int index = i;
                int prevSampleIndex = GetPreviousSample(index).GetIndex() / PointDensity;
                int nextSampleIndex = GetNextSample(index).GetIndex() / PointDensity;
                float factor = (float)(i % PointDensity) / PointDensity;

                Vector3 prevPos = new Vector3(
                   NN.GetOutput(start + (prevSampleIndex - 6) * TrajectoryDimOut + 0),
                   0f,
                   NN.GetOutput(start + (prevSampleIndex - 6) * TrajectoryDimOut + 1)
                ).GetRelativePositionFrom(nextRoot);
                Vector3 prevDir = new Vector3(
                   NN.GetOutput(start + (prevSampleIndex - 6) * TrajectoryDimOut + 2),
                   0f,
                   NN.GetOutput(start + (prevSampleIndex - 6) * TrajectoryDimOut + 3)
                ).normalized.GetRelativeDirectionFrom(nextRoot);
                Vector3 prevVel = new Vector3(
                   NN.GetOutput(start + (prevSampleIndex - 6) * TrajectoryDimOut + 4),
                   0f,
                   NN.GetOutput(start + (prevSampleIndex - 6) * TrajectoryDimOut + 5)
                ).GetRelativeDirectionFrom(nextRoot);

                Vector3 nextPos = new Vector3(
                   NN.GetOutput(start + (nextSampleIndex - 6) * TrajectoryDimOut + 0),
                   0f,
                   NN.GetOutput(start + (nextSampleIndex - 6) * TrajectoryDimOut + 1)
                ).GetRelativePositionFrom(nextRoot);
                Vector3 nextDir = new Vector3(
                   NN.GetOutput(start + (nextSampleIndex - 6) * TrajectoryDimOut + 2),
                   0f,
                   NN.GetOutput(start + (nextSampleIndex - 6) * TrajectoryDimOut + 3)
                ).normalized.GetRelativeDirectionFrom(nextRoot);
                Vector3 nextVel = new Vector3(
                   NN.GetOutput(start + (nextSampleIndex - 6) * TrajectoryDimOut + 4),
                   0f,
                   NN.GetOutput(start + (nextSampleIndex - 6) * TrajectoryDimOut + 5)
                ).GetRelativeDirectionFrom(nextRoot);

                Vector3 pos = (1f - factor) * prevPos + factor * nextPos;
                Vector3 dir = ((1f - factor) * (prevDir + RotationByCamera()) + factor * (nextDir + RotationByCamera() + AttackRotation)).normalized;
                Vector3 vel = (1f - factor) * prevVel + factor * nextVel;

                pos = Vector3.Lerp(Trajectory.Points[i].GetPosition() + vel / Framerate, pos, 0.5f);

                Trajectory.Points[i].SetPosition(
                   Utility.Interpolate(
                      Trajectory.Points[i].GetPosition(),
                      pos,
                      TrajectoryCorrection
                      )
                   );
                Trajectory.Points[i].SetDirection(
                   Utility.Interpolate(
                      Trajectory.Points[i].GetDirection(),
                      dir,
                      TrajectoryCorrection
                      )
                   );
                Trajectory.Points[i].SetVelocity(
                   Utility.Interpolate(
                      Trajectory.Points[i].GetVelocity(),
                      vel,
                      TrajectoryCorrection
                      )
                   );
            }
            start += TrajectoryDimOut * 6;

            //Compute Posture
            for (int i = 0; i < Actor.Bones.Length; i++)
            {
                Vector3 position = new Vector3(NN.GetOutput(start + i * JointDimOut + 0), NN.GetOutput(start + i * JointDimOut + 1), NN.GetOutput(start + i * JointDimOut + 2)).GetRelativePositionFrom(currentRoot);
                Vector3 forward = new Vector3(NN.GetOutput(start + i * JointDimOut + 3), NN.GetOutput(start + i * JointDimOut + 4), NN.GetOutput(start + i * JointDimOut + 5)).normalized.GetRelativeDirectionFrom(currentRoot);
                Vector3 up = new Vector3(NN.GetOutput(start + i * JointDimOut + 6), NN.GetOutput(start + i * JointDimOut + 7), NN.GetOutput(start + i * JointDimOut + 8)).normalized.GetRelativeDirectionFrom(currentRoot);
                Vector3 velocity = ControlManager.isAttack ? new Vector3(0, 0, 0) : new Vector3(NN.GetOutput(start + i * JointDimOut + 9), NN.GetOutput(start + i * JointDimOut + 10), NN.GetOutput(start + i * JointDimOut + 11)).GetRelativeDirectionFrom(currentRoot);

                Positions[i] = Vector3.Lerp(Positions[i] + velocity / Framerate, position, 0.5f);
                Forwards[i] = forward;
                Ups[i] = up;
                Velocities[i] = velocity;
            }
            start += JointDimOut * Actor.Bones.Length;

            // Basic Control working
            if (ControlManager.isBasicControl)
            {
                if (ControlManager.isDash)
                {
                    Framerate = 15;
                } 
                return;
            }
            Framerate = 30;

            if (ControlManager.jointOutput == "")
            {
                for (int i = 0; i < Actor.Bones.Length; i++)
                {
                    Actor.Bones[i].Transform.position = Positions[i];
                    Actor.Bones[i].Transform.rotation = Quaternion.LookRotation(Forwards[i], Ups[i]);

                    // left arm add rotation
                    if (i >= 9 && i <= 12)
                        Actor.Bones[i].Transform.rotation *= Quaternion.Euler(new Vector3(-90, 0, 90));

                    // left hand add rotation
                    if (i >= 11 && i <= 12)
                        Actor.Bones[i].Transform.rotation *= Quaternion.Euler(new Vector3(0, 180, 0));

                    // right arm add rotation
                    if (i >= 14 && i <= 17)
                        Actor.Bones[i].Transform.rotation *= Quaternion.Euler(new Vector3(-135, 0, -90));

                    // right hand add rotation
                    if (i >= 14 && i <= 17)
                        Actor.Bones[i].Transform.rotation *= Quaternion.Euler(new Vector3(0, -180, 0));

                    // left leg add rotation
                    if (i >= 18 && i <= 20)
                        Actor.Bones[i].Transform.rotation *= Quaternion.Euler(new Vector3(180, 180, 0));

                    // left foot add rotation
                    if (i >= 20 && i <= 20)
                        Actor.Bones[i].Transform.rotation *= Quaternion.Euler(new Vector3(135, 135, 135));

                    // left toe add rotation
                    if (i >= 21 && i <= 22)
                        Actor.Bones[i].Transform.rotation *= Quaternion.Euler(new Vector3(-90, -45, -130));

                    // right leg add rotation
                    if (i >= 23 && i <= 25)
                        Actor.Bones[i].Transform.rotation *= Quaternion.Euler(new Vector3(-180, -180, 0));

                    // right foot add rotation
                    if (i >= 25 && i <= 25)
                        Actor.Bones[i].Transform.rotation *= Quaternion.Euler(new Vector3(50, 0, 0));

                    // right toe add rotation
                    if (i >= 26 && i <= 27)
                        Actor.Bones[i].Transform.rotation *= Quaternion.Euler(new Vector3(-90, 0, 180));
                }
            }

            foreach (int i in skeletons)
            {
                socket_input += Stylied(Positions[i], Forwards[i], Ups[i], Velocities[i]) + "|";
            }

            ControlManager.jointInput = ControlManager.currentStyleIndex + "|" + socket_input;
            socket_input = "";

            if (ControlManager.jointOutput != "")
            {
                string[] output = ControlManager.jointOutput.Split(',');
                for (int i = 0; i < skeletons.Count; i++)
                {
                    Vector3 position = new Vector3(float.Parse(output[i * JointDimOut + 0]), float.Parse(output[i * JointDimOut + 1]), float.Parse(output[i * JointDimOut + 2])).GetRelativePositionFrom(currentRoot);
                    Vector3 forward = new Vector3(float.Parse(output[i * JointDimOut + 3]), float.Parse(output[i * JointDimOut + 4]), float.Parse(output[i * JointDimOut + 5])).normalized.GetRelativeDirectionFrom(currentRoot);
                    Vector3 up = new Vector3(float.Parse(output[i * JointDimOut + 6]), float.Parse(output[i * JointDimOut + 7]), float.Parse(output[i * JointDimOut + 8])).normalized.GetRelativeDirectionFrom(currentRoot);
                    Vector3 velocity = new Vector3(float.Parse(output[i * JointDimOut + 9]), float.Parse(output[i * JointDimOut + 10]), float.Parse(output[i * JointDimOut + 11])).GetRelativeDirectionFrom(currentRoot);

                    Actor.Bones[skeletons[i]].Transform.position = Vector3.Lerp(Positions[skeletons[i]] + velocity / Framerate, position, 0.5f);
                    Actor.Bones[skeletons[i]].Transform.rotation = Quaternion.LookRotation(forward, up);

                    if(skeletons[i] == 12 || skeletons[i] == 17)
                    {
                        print(i);
                        print(Actor.Bones[skeletons[i]].Transform.rotation);
                    }
                    Vector3 temp = ControlManager.tempVector[skeletons[i]];
                    Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(temp);

                    // left arm - hand
                    if (skeletons[i] == 9)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(-90, 0, 90));
                    if (skeletons[i] == 10)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(-90, 0, 90));
                    if (skeletons[i] == 11)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(90, 0, 90));

                    // right arm - hand
                    if (skeletons[i] == 14)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(-90, 0, -100));
                    if (skeletons[i] == 15)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(-90, 0, -85));
                    if (skeletons[i] == 16)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(90, 0, -90));

                    // right leg - feet
                    if (skeletons[i] == 18)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(-8, -15, -200));
                    if (skeletons[i] == 19)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(-8, -15, -197.5f));
                    if (skeletons[i] == 20)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(-65, 25, -235));
                    if (skeletons[i] == 21)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(-65, 25, -235));

                    // left leg - feet
                    if (skeletons[i] == 23)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(-5, -8, -160));
                    if (skeletons[i] == 24)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(0, -8, -160));
                    if (skeletons[i] == 25)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(-60, -52, -160));
                    if (skeletons[i] == 26)
                        Actor.Bones[skeletons[i]].Transform.rotation *= Quaternion.Euler(new Vector3(-60, -52, -160));
                }

                Actor.Bones[0].Transform.position = Positions[0];
                for (int i = 1; i < Actor.Bones.Length; i++)
                {
                    float distance = Vector3.Distance(Actor.Bones[parents[i]].Transform.position, Actor.Bones[i].Transform.position);

                    float scale = distance / distances[i];

                    Vector3 newPos = Actor.Bones[i].Transform.position - Actor.Bones[parents[i]].Transform.position; // 좌표계 변환
                    newPos /= scale; // 축소
                    newPos += Actor.Bones[parents[i]].Transform.position; // 좌표계 복구
                    Actor.Bones[i].Transform.position = newPos;

                }
            }

            //Assign Posture
            transform.position = nextRoot.GetPosition();
            transform.rotation = nextRoot.GetRotation();
        }

        private Vector3 RotationByCamera()
        {
            var camera = Camera.main;
            var forward = camera.transform.forward;
            return forward;
        }

        private float PoolBias()
        {
            float[] styles = Trajectory.Points[RootPointIndex].Styles;
            float bias = 0f;
            for (int i = 0; i < styles.Length; i++)
            {
                float _bias = Controller.Styles[i].Bias;
                float max = 0f;
                for (int j = 0; j < Controller.Styles[i].Multipliers.Length; j++)
                {
                    if (Input.GetKey(Controller.Styles[i].Multipliers[j].Key))
                    {
                        max = Mathf.Max(max, Controller.Styles[i].Bias * Controller.Styles[i].Multipliers[j].Value);
                    }
                }
                for (int j = 0; j < Controller.Styles[i].Multipliers.Length; j++)
                {
                    if (Input.GetKey(Controller.Styles[i].Multipliers[j].Key))
                    {
                        _bias = Mathf.Min(max, _bias * Controller.Styles[i].Multipliers[j].Value);
                    }
                }
                bias += styles[i] * _bias;
            }
            return bias;
        }

        private Trajectory.Point GetSample(int index)
        {
            return Trajectory.Points[Mathf.Clamp(index * 10, 0, Trajectory.Points.Length - 1)];
        }

        private Trajectory.Point GetPreviousSample(int index)
        {
            return GetSample(index / 10);
        }

        private Trajectory.Point GetNextSample(int index)
        {
            if (index % 10 == 0)
            {
                return GetSample(index / 10);
            }
            else
            {
                return GetSample(index / 10 + 1);
            }
        }

        void OnRenderObject()
        {
            if (Application.isPlaying)
            {
                if (NN.Parameters == null)
                {
                    return;
                }

                if (ShowTrajectory)
                {
                    UltiDraw.Begin();
                    UltiDraw.DrawLine(Trajectory.Points[RootPointIndex].GetPosition(), Trajectory.Points[RootPointIndex].GetPosition() + TargetDirection, 0.05f, 0f, UltiDraw.Red.Transparent(0.75f));
                    UltiDraw.DrawLine(Trajectory.Points[RootPointIndex].GetPosition(), Trajectory.Points[RootPointIndex].GetPosition() + TargetVelocity, 0.05f, 0f, UltiDraw.Green.Transparent(0.75f));
                    UltiDraw.End();
                    Trajectory.Draw(10);
                }

                if (ShowVelocities)
                {
                    UltiDraw.Begin();
                    for (int i = 0; i < Actor.Bones.Length; i++)
                    {
                        UltiDraw.DrawArrow(
                           Actor.Bones[i].Transform.position,
                           Actor.Bones[i].Transform.position + Velocities[i],
                           0.75f,
                           0.0075f,
                           0.05f,
                           UltiDraw.Purple.Transparent(0.5f)
                        );
                    }
                    UltiDraw.End();
                }

                UltiDraw.Begin();
                UltiDraw.DrawGUIHorizontalBar(new Vector2(0.5f, 0.9f), new Vector2(0.25f, 0.05f), UltiDraw.White, 0.0025f, UltiDraw.Mustard, NN.GetPhase(), UltiDraw.DarkGrey);
                UltiDraw.End();
            }
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                OnRenderObject();
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(BioAnimation_Adam))]
        public class BioAnimation_Adam_Editor : Editor
        {

            public BioAnimation_Adam Target;

            void Awake()
            {
                Target = (BioAnimation_Adam)target;
            }

            public override void OnInspectorGUI()
            {
                Undo.RecordObject(Target, Target.name);

                Inspector();
                Target.Controller.Inspector();

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(Target);
                }
            }

            private void Inspector()
            {
                Utility.SetGUIColor(UltiDraw.Grey);
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    Utility.ResetGUIColor();

                    if (Utility.GUIButton("Animation", UltiDraw.DarkGrey, UltiDraw.White))
                    {
                        Target.Inspect = !Target.Inspect;
                    }

                    if (Target.Inspect)
                    {
                        using (new EditorGUILayout.VerticalScope("Box"))
                        {
                            Target.TrajectoryDimIn = EditorGUILayout.IntField("Trajectory Dim X", Target.TrajectoryDimIn);
                            Target.TrajectoryDimOut = EditorGUILayout.IntField("Trajectory Dim Y", Target.TrajectoryDimOut);
                            Target.JointDimIn = EditorGUILayout.IntField("Joint Dim X", Target.JointDimIn);
                            Target.JointDimOut = EditorGUILayout.IntField("Joint Dim Y", Target.JointDimOut);
                            Target.ShowTrajectory = EditorGUILayout.Toggle("Show Trajectory", Target.ShowTrajectory);
                            Target.ShowVelocities = EditorGUILayout.Toggle("Show Velocities", Target.ShowVelocities);
                            Target.TargetGain = EditorGUILayout.Slider("Target Gain", Target.TargetGain, 0f, 1f);
                            Target.TargetDecay = EditorGUILayout.Slider("Target Decay", Target.TargetDecay, 0f, 1f);
                            Target.TrajectoryControl = EditorGUILayout.Toggle("Trajectory Control", Target.TrajectoryControl);
                            Target.TrajectoryCorrection = EditorGUILayout.Slider("Trajectory Correction", Target.TrajectoryCorrection, 0f, 1f);
                        }
                    }
                }
            }
        }
#endif
    }
}