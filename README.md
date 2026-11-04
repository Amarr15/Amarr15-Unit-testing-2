# Unit Testing 2 — Mini Checkout Engine

**Student:** Ammar Yasser  
**GitHub:** @Amarr15  
**Branch:** `assignment/1-11`

## Project Structure

```text
├── src/                          (Production code — NOT modified)
├── tests/
│   └── Simulation.Checkout.Tests/
│       ├── CheckoutServiceTests.cs          (Tasks 1 & 2: Business rules + Mocks)
│       ├── TestData/
│       │   ├── OrderCustomization.cs        (Task 3: AutoFixture)
│       │   ├── CustomerFaker.cs             (Task 3: Bogus)
│       │   └── TestDataTests.cs             (Task 3: Tests)
│       ├── ReceiptSnapshotTests.cs          (Task 4: Snapshot)
│       ├── *.verified.txt                   (Task 4: Approved snapshot)
│       └── ArchitectureTests.cs             (Task 5: NetArchTest)
├── coverage-report/              (Task 6: HTML coverage report)
├── TESTING_NOTES.md              (Reflection answers)
└── README.md
```

## How to Run

```bash
dotnet build
dotnet test
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"Html;TextSummary"
```
