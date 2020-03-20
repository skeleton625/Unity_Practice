# Unity Navigation

## 내비에이션 기본 동작

###1. Path Finding

3D 지오메트리를 미리 분석해 NavMesh를 생성함.

A star 알고리즘을 사용해 실시간으로 최단거리를 계산.

이동과 회전의 자동 처리 ( 기본 설정은 이동이 어색함 ).

###2. Obstacle - 장애물 회피

고정된 정적 장애물, 이동하는 동적 장애물, Agent간의 충돌 방지를 제공함.

###3. Off Mesh Link - 끊어진 Mesh를 연결하는 기능

Static Off Mesh Link 기능.

Dynamic Off Mesh Link 기능.

## 내비게이션 동작 개선

### 1. 자연스롤운 회전 구현

NavMeshAgent의 회전 기능을 제한한 뒤, 직접 회전시키는 방법이 존재함.

다음 코드는 NavMeshAgent의 자동 회전 기능을 막는 코드임.

	agent = GetComponent<NavMeshAgent>()
	
	agent.updateRotation = false;

그 뒤, NavMeshAgent의 desiredVelocity 객체를 사용해 회전을 정의해 줌.

* desiredVelocity 객체

	NavMeshAgent에게 지정해 준 좌표로 이동하기 위한 방향 값.

	기본 물리 속도 + 이동하는 방향에 장애물을 회피하는 것을 고려한 속도.

그에 대한 코드는 아래와 같음.

	private void FixedUpdate()
	
	{
		
		// sqrMagnitude : 백터의 길이에 제곱한 값을 의미 ( x * z )

		if(agent.desiredVelocity.sqrMagnitude >= 0.01f /* 0.1f x 0.1f */)

		{
			
			Vector3 direction = agent.desiredVelocity;

			Quaternion targetAngle = Quaternion.LookRotation(direction);

			transform.rotation = Quaternion.Slerp(transform.rotation, 
							      
							      targetAngle,

							      Time.deltaTime * 8.0f);

		}
	
	}

## Area Mask 활용

Area 별 경로의 가중치를 지정해줄 수 있음.

실시간으로 경로의 위치나 장애물의 위치, 다른 Agent의 위치가 바뀔 경우 활용할 수 있음.

Naviagtion 에디터의 Areas 레이어에서 Area Mask 값과 가중치를 정의할 수 있음.

### 활용 1 - 경로의 가중치

### Area Mask 경로별 비용 지정

* Area Cost

	경로의 난이도를 설정해 Agent가 경로를 선택할 때, 가장 작은 비용의 길로 이동함.

### 가중치 설정

경로(Object)마다 Area Mask를 지정해 경로 별 비용을 지정함.

Bake를 했을 때, 경로의 가중치에 따라 다른 색상으로 경로가 표시됨.

	1. 실제로 Agent가 주어진 좌표로 이동할 때,  두 가지 경로의 가중치가 존재하면

	   가중치 비용이 낮은 경로 (Object)를 통해 좌표로 이동하게 됨.

### 활용 2 - 경로의 변경

### 이동할 수 있는 영역을 지정하는 기능

가중치와는 다르게 런타임에 이동할 수 있는 영역을 설정함.

	1. 다리를 사용할 때, 다리 양 옆은 A Area로 다리 중앙을 B Area로 설정한다 가정.

	2. A의 가중치를 1로, B의 가중치를 2로 정의.

	3. NavMeshAgent 컴포넌트에 Area Mask 속성이 존재.
	   
	   이 속성을 통해 어떤 Area Mask를 적용할 것인지 정의할 수 있음.

3번의 경우 스크립트를 통해 아래 코드처럼 변경해줄 수 있음.

	private void SetAreaMask(bool isUp)
	
	{

		foreach(GameObject enemy in enemies)
		
		{

			// areaMask 변수는 비트마스크로 정의.

			// ~ : not 연산자

			enemy.GetComponent<NavMeshAgent>().areaMask = isUp ? ~(1<<7) : ~0;

		}

	}

* foreach 문 내의 코드 의미

	1. bool 변수 isUp이 true일 경우

		7번 레이어를 제외한 1번 ~ 6번 레이어를 Area Mask로 지정.

		areaMask = 0011 1111(2)

	2. bool 변수 isUp이 false일 경우

		1번 ~ 7번 레이어를 Area MAsk로 지정.

		areaMask = 0111 1111(2)


## 우선순위 - Priority

Field 내에 여러 Agent가 존재하고 그 Agent들이 한 좌표로 이동할 때, Race Condition이 발생할 수 있음.

Race Condition을 방지하기 위해 NavMeshAgent의 우선 순위 속성을 사용할 수 있음.

* Race Condition

	같은 좌표로 이동하기 위해 다른 Agent들과 부딛히거나 밀어내는 상황.

	즉 Agent 간의 병목 현상이 일아나게 됨.

## 내비게이션의 중요한 속성들

	updatePosition 		: 목적지까지 위치를 자동으로 동기화하는 옵션

	updateRotation 		: 이동 중, 자동으로 회전시키는 옵션

	remainingDistance 	: 목적지까지 남은 거리

	velocity 		: Agent의 현재 속도

	desireVelocity 		: 장애물 회피를 고려한 속도( 잠재적 속도 )

	pathPending 		: 목적지까지의 최디ㅏㄴ거리 계산이 완료됐는지 여부

	isPathStale 		: 계산한 경로의 유효성 여부( 동적 장애물, Off Mesh Link )

* NavMeshAgent를 거리 계산 용도로만 사용하고 좌표 이동, 회전을 개발자가 직접 정의하도록 구현할 수도 있음.

	updatePosition, updateRotation, remainingDistance 조정

* NavMeshAgent를 사용한 이동, 회전 처리를 할 때, desiredVelocity를 이용해햐 함.

* pathPanding, isPathStale 속성을 통해 이동에 따른 예외처리를 구현할 수 있음.

# 확장 내비게이션 ( New Navigation System - 임시 )

해당 내비게이션 시스템은 유니티의 패키지 매니저(Package Manager)에 존재하지 않음.

유니티 Navigation 에디터에 링크로 존재함.

	링크 : https://github.com/Unity-Technologies/NavMeshComponents

Github 내 Assets/NavMeshComponents에 필용한 컴포넌트 스크립트들이 존재함

	1. NavMeshSurface

	2. NavMeshLink

	3. NavMeshModifier

	4. NavMeshModifierVolume

## NavMeshSurface 컴포넌트

1. NavMeshSurface 컴포넌트 내에서 바로 Bake를 진행할 수 있음.

2. 장애물 및, 바닥 Object에 대해 Navigation Static 플래그를 체크하지 않아도 됨.

3. Object를 기준으로 Bake되기 때문에, Object를 회전하면 NavMesh 또한 회전하게 됨.

4. Include Layers에 포함된 레이어는 한 번에 Bake해 NavMesh를 생성할 수가 있음.

## NavMeshLink 컴포넌트

1. 기존의 Off Mesh Link와 유사함.

2. 수직의 벽으로도 이동할 수 있음.

3. Scene view에서 직접 연결 위치와 폭을 설정 가능.

## NavMeshModifier

1. 자식 Object들을 동시에 특정 NavMesh 속성을 부여할 수 있음.






