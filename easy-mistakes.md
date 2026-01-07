## Mistake 1
```
- StaticResource vs DynamicResource: StaticResource는 XAML 파싱 시점에 리소스를 찾고, DynamicResource는 런타임에 리소스를 찾습니다.
- 재귀 템플릿 패턴: 트리 구조 데이터를 표시할 때 자기 자신을 참조하는 템플릿은 반드시 DynamicResource를 사용해야 합니다.
```


## Mistake 2
```
  1. WPF MinHeight vs Height: MinHeight 속성은 숫자 값만 허용합니다. "Auto"는 Height 속성에서만 유효합니다
  2. Border 자동 크기: MinHeight 없이도 Border는 자식 컨텐츠에 맞게 자동으로 크기가 조정됩니다
```