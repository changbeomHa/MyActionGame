# MyActionGame
 Capstone Design Project

* 이 레포지토리에는 게임에 대한 파일만 존재합니다. Motion Puzzle & PFNN에 대한 파일은 다른 레포지토리를 참고합니다.


[![Video Label](https://img.youtube.com/vi/kNbZ1Jq-2Ss/0.jpg)](https://youtu.be/kNbZ1Jq-2Ss)

시작 부분에서는 기술을, 1분46초 부터 게임을 소개합니다.
<br/>


### 스토리
어디에나 존재하는 <프로토타입 봇>, 신작 게임의 메인 캐릭터 선발 시험에 지원하다.
시험장 입구에서 그동안 훈련했던 기억을 되짚어보고, 회상을 끝낸 후 시험장에 입장한다.


### 게임 개발에 사용된 기술
1. 부위별 피격
- 피격 부위 & 적중 파워에 따라 출력하는 피격모션이 다름 
2. AI 기술
- 플레이어가 적 AI의 움직임을 보고 적합한 액션을 취할 수 있도록 설계
- 플레이어의 행동을 살피며 판단하는 AI (대기, 추격 및 후퇴, 상반신 타격, 하반신 타격, 반격)
   - 대기상태에서 공격 변수가 할당되면 공격 준비 상태에 들어감. 공격변수는 일정한 시간 내에 반드시 할당됨
   - 플레이어를 추격해 공격가능 범위가 되면 공격
   - 공격 후 플레이어를 살피며 플레이어와 다시 거리를 벌리기 위해 후퇴함
   - 대기 상태에서 플레이어와 거리를 유지하며 오른쪽, 왼쪽으로 움직임
   - 피격 상태가 지속되면 플레이어에게 반격하여 플레이어의 공격 흐름을 끊음
3. TPS형 카메라 움직임에서 효과적인 액션을 위한 적 탐지 기능
- 단순한 콜라이더 충돌이 아닌 타겟 설정과 방향 고정을 통해 조작감을 개선
- 플레이어와 타겟 적의 방향이 고정되어 타격확률을 극대화. 
- 다수의 적의 경우 플레이어의 앞쪽 방향에 있는 적을 우선타겟으로 설정


### 게임 진행
인트로 -> 튜토리얼 -> 훈련 -> 보스 -> 엔딩


### 보스 패턴 설명 (영상 순서대로)
1. 주먹을 땅으로 내려찍어 4방향에 바위블록을 설치한다. 4개의 바위블록 중 하나의 바위에서 미니 보스몹이 탄생한다.
플레이어는 블록 생성 라인으로 바위 위치를 파악하고, 바위블록 근처에서 G키를 눌러 바위의 폭발을 억제할 수 있다. 
2. 보스가 높이 점프해 플레이어 위치로 착지한다. 이 때 낙하 예상 범위가 그려지며, 플레이어가 낙하 범위에 있다면 
큰 데미지를 받고 앞으로 넘어진다.
3. 힘을 모았다가 플레이어 위치로 레이저 빔을 발사한다. 레이저 발사 동안 보스는 천천히 플레이어 방향으로 회전한다.
레이저에 적중시 1초 단위로 데미지를 받는다.
4. 땅에서 바위를 뽑아 플레이어 위치로 던진다. 바위는 보스 위치와 플레이어 위치 사이를 포물선을 그리며 날아간다.
플레이어가 바위와 충돌하면 뒤로 넘어지며 데미지를 받고, 바위가 파괴된다.


### 게임의 고유한 매커니즘
1. 심박수 시스템 : 정확한 타이밍에 노드를 맞춰 심박수를 유지하도록 한다. 심박수를 맞추지  오차범위에 따라 1~3 단위로 증가한다. 심박수가 너무 높아지면 기절상태에 빠지며,
심박수를 낮게 유지하면 집중상태에서 치명타 공격을 가할 수 있다.
2. 방어막 시스템 : 모든 적은 방어막을 갖는다. 방어막은 치명타 공격으로만 파괴할 수 있다.
