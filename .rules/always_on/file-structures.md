
## MVVM
- 페이지 별로 별도의 폴더를 정의하고 폴더 내부에 Views, ViewModels 폴더를 정의하여 파일을 내부에 생성한다.. 모든 Model 및 DTO는 `.csproj`가 있는 프로젝트 최상위 디렉토리의 `Models` 폴더에 정의한다 (내부 Subdirectory는 허용).

## 예외
만약 하나의 View(.xaml, .xaml.cs)와 하나의 ViewModel파일로만 구성되어 있는 경우 페이지 폴더 내부에 Views와 ViewModels 폴더를 생성하지 않고 바로 파일을 생성한다.

## 예시
```
Views/
├── Dashboard/
│   ├── Views 
│   │   ├── DashboardView.xaml
│   │   ├── DashboardView.xaml.cs
│   │   ├── UserView.xaml
│   │   └── UserView.xaml.cs
│   └── ViewModels 
│       ├── DashboardViewModel.cs
│       └── UserViewModel.cs
Models/
├── Dashboard/
│   ├── Models 
│   │   └── DashboardModel.cs
│   └── DTOs 
│       └── DashboardDTO.cs
```


## 예외 경우 예시
```
Views/
├──Dashboard
│   ├── DashboardView.xaml
│   ├── DashboardView.xaml.cs
│   └── DashboardViewModel.cs
Models/
├── DashboardModel.cs
└── DashboardDTO.cs
```

