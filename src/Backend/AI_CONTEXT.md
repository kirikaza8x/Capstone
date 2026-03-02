# PROJECT CONTEXT: AIPromo Event System (SP26SE115)

## 1. Project Overview
* **Name:** AIPromo (Event Management & AI Marketing System).
* **Goal:** A centralized platform for creating events, digital ticketing (QR), and AI-driven marketing assistance for organizers.
* **Business Model:** B2B2C (Organizers create events, Attendees buy tickets).

## 2. Technical Stack
* **Framework:** .NET 9.
* **Database:** PostgreSQL (Multi-Schema approach).
* **API Style:** Minimal API.
* **ORM:** Entity Framework Core 9.

## 3. Architecture: Modular Monolith
The system is divided into strictly isolated modules.
* **Pattern:** Clean Architecture (Domain layer -> Application layer -> Infrastructure layer -> Presentation layer).
* **Communication:**
    * **Intra-module:** Direct method calls.
    * **Inter-module:** Asynchronous via **Integration Events** (using Outbox/Inbox Pattern) or strictly defined public interfaces. **NO direct DbContext sharing between modules.**
* **Design Patterns:** DDD (Domain-Driven Design), CQRS (using MediatR), Repository Pattern, Result Pattern (for error handling).

## 4. Domain Modules & Entities
*Each module has its own DbContext and Schema.*

### Module 1: Identity (Auth & Profiles)
* **Responsibility:** Authentication, Authorization, User info.
* **Entities:** `User`, `Role` (Admin, Staff, Organizer, Attendee), `UserRole`, `UserSession`, `RefreshToken`, `OrganizerProfile`, `Wallet`.

### Module 2: Event (Venue, Schedule & Catalog)
* **Responsibility:** Event lifecycle, schedule (sessions), venue layout (areas/seats), and ticket pricing/types.
* **Entities:**
  * **Core:** `Event`, `EventCategory`, `EventCategoryMapping`, `EventImage`, `EventActorImage`, `EventTag`, `EventStaff`.
  * **Venue & Schedule:** `EventSession` (Suất diễn), `Area` (Khu vực: Zone A, Rạp 1...), `Seat` (Ghế vật lý A01, A02...).
  * **Inventory (Quan trọng):** `SessionSeatStatus` (Lưu trạng thái ghế cho từng suất diễn: Available, Locked, Sold).
  * **Sales Catalog:** `TicketType` (Loại vé bán ra: VIP, Thường. Có Enum Type: `0 = SEAT`, `1 = ZONE`).

### Module 3: Ticketing (Orders & Transactions)
* **Responsibility:** Order processing, Ticket generation (QR Code), and Inventory deduction.
* **Entities:** * `Order` (Đơn hàng tổng).
  * `OrderTicket` (Vé vật lý/QR Code của user. Bắt buộc có `TicketTypeId`. Nếu là vé ngồi sẽ có thêm `SeatId`, nếu là vé Zone đứng thì `SeatId` = NULL).
  * `OrderVoucher`, `Voucher`.

### Module 4: Payment (Finance)
* **Responsibility:** Wallet management, Transaction processing, Payment Gateway Integration (VNPay, MoMo).
* **Entities:** `Wallet`, `WalletTransaction`, `Payment`.

### Module 5: Marketing (AI Core)
* **Responsibility:** AI content generation, Analytics, Audience Segmentation.
* **Entities:** `MarketingContent`, `MarketingAnalytics`, `UserInterestScores`, `MarketingAudienceSegments`, `UserPrompt`.

### Module 6: System (Configuration)
* **Responsibility:** Logging, Global settings, Policies.
* **Entities:** `Config`, `Log`, `Policy`.

## 5. Coding Standards & Rules (Strict Enforcement)

### General
* **SOLID:** Adhere strictly to SOLID principles.
* **Naming:** PascalCase for Classes/Methods, camelCase for variables/parameters.
* **Async:** Use `async/await` for all I/O operations.

### .NET 9 Specifics
* Use **Primary Constructors** for dependency injection.
* Use `collection expressions []`.


## 6. Instruction for AI Assistant
* **Role:** Act as a Senior Software Architect and Lead Developer.
* **Task:** When asked to write code, always check which **Module** the feature belongs to.
* **Constraint:** If a feature requires data from another module (e.g., *Create Order* needs *Event* data), simulate a check via an Interface or Integration Event, do not query the other module's table directly.
* **Output:** Provide the file path/structure for every code snippet generated (e.g., `src/Modules/Event/Application/Commands/CreateEvent/CreateEventCommandHandler.cs`).