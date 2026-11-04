# Testing Notes

## 1. Test Results Summary
- **Total tests:** 20
- **Passing:** 20
- **Failing:** 0

## 2. Line Coverage
- **Overall Line Coverage:** 90.5% (Covered lines: 106 / 117 coverable lines)
- **Domain Layer Coverage:** 100%
- **Application Layer Coverage:** 95.8%

*(Extracted directly from `coverage-report/Summary.txt`)*

## 3. Libraries Used
- **Moq:** Used in `CheckoutServiceTests` to mock external dependencies (`IInventoryService`, `IOrderRepository`, `INotificationService`) and verify interaction counts (`Times.Once`, `Times.Never`) during success and failure paths.
- **AutoFixture:** Used in `TestDataTests` with a custom `ValidOrderCustomization` to generate random, valid `Order` and `OrderItem` hierarchies with positive quantities and prices.
- **Bogus:** Used in `CustomerFaker` with a deterministic fixed seed (`42`) to generate realistic customer names and email addresses to verify that the checkout receipt preserves customer details consistently.

## 4. Business Rules Assessment (Examples B & C)
No, it is not fair because a strict cliff threshold creates an incentive distortion where purchasing items worth 999.99 EGP costs 999.99 EGP (0 discount), whereas spending just 0.01 EGP more to reach 1,000 EGP triggers a 10% discount (100 EGP) resulting in a net cost of 900 EGP.

## 5. Potential Bugs Identified
None.
