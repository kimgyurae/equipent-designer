## Mistake 1
- StaticResource vs DynamicResource: StaticResource는 XAML 파싱 시점에 리소스를 찾고, DynamicResource는 런타임에 리소스를 찾습니다.
- 재귀 템플릿 패턴: 트리 구조 데이터를 표시할 때 자기 자신을 참조하는 템플릿은 반드시 DynamicResource를 사용해야 합니다.


## Mistake 2
1. WPF MinHeight vs Height: MinHeight 속성은 숫자 값만 허용합니다. "Auto"는 Height 속성에서만 유효합니다
2. Border 자동 크기: MinHeight 없이도 Border는 자식 컨텐츠에 맞게 자동으로 크기가 조정됩니다


## Mistak3
- ToastService를 이용하여 Localized messge를 보여줄때는 .resx 파일에 키를 추가한 후 반드시 Strings.Designer.cs에도 해당 속성이 존재하는지 확인해야 함. (Visual Studio 외부에서 편집 시 자동 생성되지 않음).

```
ToastService.Instance.ShowError(
                        Strings.DeleteHardware_CannotDelete_Title,
                        Strings.DeleteHardware_CannotDelete_MinChild);

```

### Mistake 4
- 마우스 또는 키보드 action handler를 추가할 때 기존에 동일 한 action handler를 비활성화하거나 override하지 않게 주의할 것.


### Mistake 5
- WPF에서 모달리스 UI 요소(floating buttons, popups 등)를 사용할 때는 포커스 관리가 중요. 특히 키보드 단축키가 특정 컨테이너의 PreviewKeyDown에 바인딩되어 있을 때, 해당 컨테이너 외부 요소와 상호작용 후에는 명시적으로 포커스를 복원해야 함.