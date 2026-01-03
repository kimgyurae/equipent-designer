
# C# 테스트 규칙

## 1. 테스트 프로젝트 구조

### 1.1 테스트 프로젝트 명명 규칙

테스트 프로젝트는 테스트 대상 프로젝트 이름 뒤에 `.Tests` 접미사를 붙여 명명한다.

| 대상 프로젝트 | 테스트 프로젝트 |
|--------------|----------------|
| `MyApp.Core` | `MyApp.Core.Tests` |
| `MyApp.Services` | `MyApp.Services.Tests` |
| `MyApp.Infrastructure` | `MyApp.Infrastructure.Tests` |

### 1.2 디렉토리 구조 규칙

테스트 파일은 테스트 대상 코드의 디렉토리 구조를 **동일하게 미러링**하여 배치한다.

**예시:**

```
src/
├── MyApp.Core/
│   ├── Services/
│   │   ├── UserService.cs
│   │   └── OrderService.cs
│   ├── Repositories/
│   │   └── UserRepository.cs
│   └── Helpers/
│       └── StringHelper.cs

tests/
├── MyApp.Core.Tests/
│   ├── Services/
│   │   ├── UserServiceTests.cs
│   │   └── OrderServiceTests.cs
│   ├── Repositories/
│   │   └── UserRepositoryTests.cs
│   └── Helpers/
│       └── StringHelperTests.cs
```

### 1.3 테스트 파일 명명 규칙

테스트 파일명은 테스트 대상 클래스명 뒤에 `Tests` 접미사를 붙인다.

| 대상 클래스 파일 | 테스트 파일 |
|-----------------|------------|
| `UserService.cs` | `UserServiceTests.cs` |
| `OrderRepository.cs` | `OrderRepositoryTests.cs` |
| `EmailValidator.cs` | `EmailValidatorTests.cs` |

---

## 2. 테스트 케이스 네이밍 컨벤션

### 2.1 기본 명명 패턴

테스트 메서드명은 다음 패턴을 따른다:

```
[테스트대상메서드]_[시나리오/조건]_[기대결과]
```

**구성 요소:**

| 구성 요소 | 설명 | 예시 |
|----------|------|------|
| 테스트대상메서드 | 테스트하려는 메서드 또는 기능의 이름 | `CreateUser`, `CalculateTotal` |
| 시나리오/조건 | 테스트가 실행되는 특정 조건이나 입력 상태 | `WithValidInput`, `WhenUserNotFound` |
| 기대결과 | 테스트가 기대하는 결과나 동작 | `ReturnsTrue`, `ThrowsException` |

### 2.2 명명 패턴 상세 예시

#### 2.2.1 성공 케이스

```csharp
// 패턴: [Method]_[Condition]_[ExpectedResult]

[Fact]
public void CreateUser_WithValidUserData_ReturnsCreatedUser()

[Fact]
public void CalculateDiscount_WhenCustomerIsPremium_ReturnsDiscountedPrice()

[Fact]
public void GetUserById_WithExistingUserId_ReturnsUser()

[Fact]
public void SendEmail_WithValidEmailAddress_ReturnsTrue()
```

#### 2.2.2 실패/예외 케이스

```csharp
[Fact]
public void CreateUser_WithNullEmail_ThrowsArgumentNullException()

[Fact]
public void GetUserById_WithNonExistentId_ReturnsNull()

[Fact]
public void ProcessPayment_WhenInsufficientBalance_ThrowsInsufficientFundsException()

[Fact]
public void ValidateInput_WithEmptyString_ReturnsFalse()
```

### 2.5 복합 조건 케이스

복잡한 시나리오의 경우 `And`를 사용하여 조건을 연결할 수 있다:

```csharp
[Fact]
public void ProcessOrder_WithValidItemsAndSufficientStock_CreatesOrder()
```

---

## 3. 테스트 클래스 구조

### 3.1 테스트 클래스 명명

```csharp
public class UserServiceTests
{
    // UserService에 대한 모든 테스트
}
```

### 3.2 내부 클래스를 활용한 그룹화 (선택사항)

특정 메서드에 대한 테스트가 많은 경우, 내부 클래스로 그룹화할 수 있다:

```csharp
public class UserServiceTests
{
    public class CreateUserMethod
    {
        [Fact]
        public void WithValidUserData_ReturnsCreatedUser() { }
        
        [Fact]
        public void WithNullEmail_ThrowsArgumentNullException() { }
        
        [Fact]
        public void WithDuplicateEmail_ThrowsDuplicateEmailException() { }
    }
    
    public class GetUserByIdMethod
    {
        [Fact]
        public void WithExistingId_ReturnsUser() { }
        
        [Fact]
        public void WithNonExistentId_ReturnsNull() { }
    }
}
```

---

## 4. 추가 가이드라인

### 4.1 언어 규칙

- 테스트 메서드명은 **영어**로 작성한다.
- **PascalCase**를 사용하되, 각 구성 요소는 **언더스코어(_)**로 구분한다.
- 축약어 사용을 피하고 명확한 표현을 사용한다.

### 4.2 테스트 메서드 본문 구조 (AAA 패턴)

```csharp
[Fact]
public void CreateUser_WithValidUserData_ReturnsCreatedUser()
{
    // Arrange (준비)
    var userData = new UserCreateDto { Name = "홍길동", Email = "hong@example.com" };
    var sut = new UserService(_mockRepository.Object);
    
    // Act (실행)
    var result = sut.CreateUser(userData);
    
    // Assert (검증)
    Assert.NotNull(result);
    Assert.Equal("홍길동", result.Name);
}
```
