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
* **Mapping:** AutoMapper.
* **Caching & Distributed Lock:** Redis (Used for locking seats and temporary session data).

## 3. Architecture: Modular Monolith
The system is divided into strictly isolated modules.
* **Pattern:** Clean Architecture (Domain layer -> Application layer -> Infrastructure layer -> Presentation layer).
* **Communication:**
    * **Intra-module:** Direct method calls.
    * **Inter-module:** Asynchronous via **Integration Events** (using Outbox/Inbox Pattern) or strictly defined public interfaces. **NO direct DbContext sharing between modules.**
* **Design Patterns:** DDD (Domain-Driven Design), CQRS (using MediatR), Repository Pattern, Result Pattern (for error handling).

## 4. Domain Modules & Entities
*Each module has its own DbContext and Schema.*

---

### Module 1: Users (Auth & Profiles)
* **Responsibility:** Authentication, Authorization, User profile management.
* **Entities:**
  * `User` — Core user account. Fields: `email`, `password`, `name`, `birthday`, `gender`, `status` (enum: `active`, `inactive`, `banned`), `phone`, `address`, `description`, `social_link`.
  * `Role` — System roles: `Admin`, `Staff`, `Organizer`, `Attendee`.
  * `UserRole` — Many-to-many mapping between `User` and `Role`.
  * `UserSession` — Tracks active login sessions. Fields: `user_id`, `device_type`, `source`, `campaign_id`, `created_at`, `last_active_at`.
  * `RefreshToken` — Fields: `token`, `expired_at`, `user_id`.
  * `OrganizerProfile` — Extended profile for organizer accounts. Fields: `logo`, `display_name`, `account_name`, `account_number`, `bank_code`, `branch`, `business_type` (enum: `company`, `individual`), `address`, `tax_code`, `identity_number`, `business_name`, `social_link`, `status` (enum: `pending`, `verified`, `rejected`), `verified_at`, `type` (enum: `management`, `check_in`), `user_id`.

---

### Module 2: Event (Venue, Schedule & Catalog)
* **Responsibility:** Event lifecycle, schedule (sessions), venue layout (areas/seats), ticket pricing/types, and hashtag management.

* **Business Rules:**
  * Mỗi **Event** = 1 đêm diễn tại 1 địa điểm cố định. Nếu tổ chức ở địa điểm khác hoặc ngày khác → tạo Event mới.
  * **EventSession** = các suất diễn trong cùng 1 Event (ví dụ: suất 19h, suất 21h).
  * **TicketType** (VIP, Basic...) thuộc về **Event**, áp dụng chung cho tất cả EventSession trong Event đó. Giá và khu vực không thay đổi theo suất.
  * Khi mua vé, user chọn **TicketType** (loại vé gì) + **EventSession** (đi suất nào) — hai chiều độc lập nhau.

* **Entities:**

  * **Core:**
    * `Event` — Fields: `organizer_id`, `title`, `status` (enum: `Draft`, `PendingReview`, `Published`, `Suspended`, `PendingCancellation`, `Cancelled`, `Completed`), `ticket_sale_start_at`, `ticket_sale_end_at`, `event_start_at`, `event_end_at`, `description`, `banner_url`, `location`, `map_url`, `policy`, `spec` (JSONB), `url_path`, `is_email_reminder_enabled`, `reminder_triggered_at` (TIMESTAMP, NULLABLE), `cancellation_reason` (TEXT, NULLABLE), `suspension_reason` (TEXT, NULLABLE), `suspended_at` (TIMESTAMP, NULLABLE), `suspended_by` (GUID, NULLABLE).
      * **Event Status State Machine:**
        * `Draft` → `PendingReview` — Organizer submit để duyệt
        * `PendingReview` → `Published` — Staff approve
        * `PendingReview` → `Draft` — Staff reject (kèm note lý do)
        * `Published` → `Suspended` — Staff suspend (ghi `suspension_reason` + `suspended_at`). Dừng bán vé mới, vé cũ vẫn Valid.
        * `Published` → `PendingCancellation` — Organizer request hủy (ghi `cancellation_reason`)
        * `Published` → `Completed` — System tự động khi `event_end_at <= utcNow`
        * `Suspended` → `PendingReview` — Organizer sửa thông tin + re-submit
        * `Suspended` → `Cancelled` — Staff cancel trực tiếp → trigger refund (Organizer **không** được request cancel từ Suspended)
        * `PendingCancellation` → `Cancelled` — Staff approve → trigger refund
      * **Transition rules:**
        * **Staff:** approve/reject `PendingReview`, suspend `Published`, cancel trực tiếp từ `Suspended`, approve `PendingCancellation`.
        * **Organizer owner:** submit `Draft` → `PendingReview`, re-submit `Suspended` → `PendingReview`, request cancel `Published` → `PendingCancellation` (kèm `cancellation_reason`).
        * **System (Quartz job):** tự động `Published` → `Completed` mỗi 5 phút khi `event_end_at <= utcNow`.
        * **Khi Suspended:** bán vé mới bị block, vé đã bán vẫn Valid. Nếu sau đó bị Cancelled → trigger refund toàn bộ.
    * `EventCategory` — Fields: `code` (ENUM), `name`, `description`, `is_active`.
    * `EventCategoryMapping` — Many-to-many: `event_id`, `category_id`.
    * `EventImage` — Fields: `event_id`, `image_url`.
    * `EventActorImage` — Lưu thông tin diễn viên/nghệ sĩ của event. Fields: `event_id`, `name`, `major`, `image`.
    * `EventMember` — Thành viên được Organizer assign vào event. Fields: `event_id`, `user_id`, `permissions` (text[]), `status`, `assigned_by`, `created_at`.
      * **Member phải là user đã đăng ký trong hệ thống** (bất kỳ role nào).
      * **`permissions` là array of enum strings.** Các giá trị hợp lệ:
        * `CheckIn` — Scan QR, manual check-in tại cổng vào.
        * `ViewReports` — Xem thống kê check-in, số vé đã bán, doanh thu của event.
      * **Chỉ Organizer owner** mới được assign/remove member và thay đổi permissions.
      * Các action nhạy cảm (publish, xóa event, quản lý tài chính) **không được phân quyền** cho member — chỉ owner.
    * `Hashtag` — Fields: `name`, `slug`, `usage_count`.
    * `EventHashtag` — Many-to-many: `event_id`, `hashtag_id`.

  * **Venue & Schedule:**
    * `EventSession` — Suất diễn, thuộc `Event` (`event_id`). Fields: `title`, `description`, `start_time`, `end_time`.
    * `Area` — Khu vực của venue (`event_id`). Fields: `name`, `capacity`, `type` (enum: `zone`, `seat`, `default`).
      * **`Area.Type` Enum:**
        * `zone` — Khu vực bán theo khu, không chia ghế cụ thể (standing, zone vé khu...).
        * `seat` — Khu vực có sơ đồ ghế cụ thể, mỗi `Seat` được định danh riêng.
        * `default` — Auto-generated bởi backend khi event có ghế ngồi nhưng organizer không chia khu vực. FE không tương tác trực tiếp với loại này.
      * *Business Rule:* Nếu event có ghế ngồi nhưng không chia khu vực, backend **tự động tạo một Default Area** (`type = default`) và gán `Seat` vào đó.
    * `Seat` — Ghế vật lý (`area_id`). Fields: `seat_code`, `row_table`, `column_table`, `x` (float), `y` (float). Chỉ tồn tại trong `Area.type = seat`.
      * **Không có `status` field** — ghế được coi là luôn available, trạng thái chiếm dụng được tính động qua `OrderTicket` và Redis lock.

  * **Sales Catalog:**
    * `TicketType` — Loại vé của Event (`event_id`). Fields: `name`, `price` (Type), `quantity` (int), `sold_quantity` (int), `type` (enum: `zone`, `seat`), `area_id` (GUID, NULLABLE).
      * **`TicketType.AreaId` là NULLABLE.** FE có thể tạo `TicketType` trước khi gán `Area` (flow 2 bước). Backend chỉ cho phép **Publish event** khi tất cả `TicketType` đã có `AreaId`.
      * Type của vé **derive từ `Area.Type`** được gán. `Area.zone` → vé không gắn ghế, `Area.seat` → vé gắn `SeatId` cụ thể.
      * **`TicketType.quantity` áp dụng chung cho tất cả EventSession trong Event.** Không track quota riêng theo từng suất.

* **Relationship Summary:**
* **Domain Events:**
* `EventPublishedDomainEvent` → raises `EventPublishedIntegrationEvent` (→ Marketing module)
* `EventCancelledDomainEvent` → raises `EventCancelledIntegrationEvent` (→ Ticketing module trigger refund)
* `TicketTypeSoldOutDomainEvent` → raises `TicketTypeSoldOutIntegrationEvent` (→ Marketing module)

* **Background Jobs (Quartz):**
* `AutoCompletePublishedEventsJob` — Chạy mỗi 5 phút. Query `Published` events có `event_end_at <= utcNow`, batch 50. Gọi `event.Complete()` → raises `EventCompletedDomainEvent`.
* `SendEventReminderJob` — Chạy mỗi 1 giờ. Query `Published` events có `is_email_reminder_enabled = true`, `reminder_triggered_at IS NULL`, `event_start_at` trong window `[utcNow, utcNow+24h]`, batch 100. Gọi `event.MarkReminderTriggered()` → set `reminder_triggered_at`.

* **Inventory Calculation (Dynamic, no cached status table):**
* **SEAT:** Available = Total `Seat` thuộc `Area` - `OrderTicket` có `seat_id` đó (Valid/Used) - Locked `seat_id` trong Redis.
* **ZONE:** Available = `TicketType.quantity` - COUNT `OrderTicket` theo `ticket_type_id` (Valid/Used) - Locked count trong Redis. *(Quota áp dụng chung, không phân biệt session.)*

---

### Module 3: Ticketing (Orders, Transactions & Check-in)
* **Responsibility:** Order processing, Ticket generation (QR Code), inventory deduction, and check-in management.

* **Entities:**
* `Order` — Đơn hàng tổng. Fields: `user_id`, `total_price`, `type`, `status` (enum: `Pending`, `Paid`, `Cancelled`).
  * **`Order.type`** — Phân loại đơn hàng. *(Cần confirm enum values với team — ví dụ: `Standard`, `Complimentary`...)*
* `OrderTicket` — Vé vật lý/QR Code của user. Fields: `order_id`, `event_session_id`, `ticket_type_id`, `seat_id` (NULLABLE), `QRCode`, `status` (enum: `Valid`, `Used`, `Cancelled`), `checked_in_at` (TIMESTAMP, NULLABLE), `checked_in_by` (GUID, NULLABLE — staff `user_id`).
  * Bắt buộc có `TicketTypeId` — loại vé gì (VIP/Basic).
  * Bắt buộc có `EventSessionId` — đi suất nào (19h/21h).
  * Nếu `TicketType` thuộc `Area.type = seat` → có thêm `SeatId` (ghế cụ thể).
  * Nếu `TicketType` thuộc `Area.type = zone` → `SeatId = NULL`.
  * **This is the Single Source of Truth for sold inventory.**
* `Voucher` — Fields: `coupon_code`, `type`, `condition`, `value`, `total_use`, `max_use_per_user`, `start_date`, `end_date`.
* `OrderVoucher` — Fields: `order_id`, `voucher_id`, `discount_amount`, `applied_at`.

* **Check-in Flow:**
1. Staff scan QR code (hoặc manual nhập mã).
2. Hệ thống tìm `OrderTicket` theo `QRCode`.
3. Validate: `Status = Valid` và `EventSessionId` khớp với session đang diễn ra.
4. Update: `Status = Used`, ghi `checked_in_at = now()`, `checked_in_by = staff_user_id`.
5. Manual override: Staff có thể check-in thủ công khi QR lỗi, vẫn ghi `checked_in_by`.

* **Order Expiry Flow (Redis Lock):**
* Khi user bắt đầu đặt vé → lock seat/zone slot trong Redis với TTL = 15 phút.
* `Order` được tạo với `Status = Pending`.
* Nếu thanh toán thành công trong TTL → `Order = Paid`, release lock, deduct inventory.
* Nếu hết TTL mà chưa thanh toán → background job cancel `Order`, release Redis lock.
* Redis key convention:
  * Seated: `seat_lock:{event_session_id}:{seat_id}` → value = `user_id`
  * Zone: `zone_lock:{event_session_id}:{ticket_type_id}` → value = locked count

---

### Module 4: Payment (Finance)
* **Responsibility:** Wallet management, transaction processing, payment gateway integration (VNPay, MoMo), and financial ledger.

* **Entities:**
* `Wallet` — Fields: `user_id`, `balance`, `direction`, `status`, `balance_before`, `balance_after`, `note`, `updated_at`.
* `WalletTransaction` — Fields: `wallet_id`, `type` (enum: `Deposit`, `Withdrawal`, `Payment`, `Refund`), `amount`, `balance_before`, `balance_after`, `note`, `status` (enum: `Pending`, `Success`, `Failed`), `metadata` (JSON).
* `Payment` — Fields: `order_id`, `method` (enum: `Bank_Transfer`, `E_Wallet`), `status` (enum: `Pending`, `Success`, `Failed`).
* `PaymentTransaction` — Lưu raw data từ payment gateway. Fields: `payment_id`, `gateway` (enum: `vnpay`, `momo`), `gateway_txn_id`, `amount` (decimal), `status` (enum: `pending`, `success`, `refund`, `failed`), `raw_response` (JSONB), `created_at`.

* **Note:** `PaymentTransaction` là audit log không thể thay đổi — mỗi callback từ gateway tạo 1 record mới, không update.

---

### Module 5: Marketing (AI Core)
* **Responsibility:** AI content generation, analytics tracking, audience segmentation, user behavior analysis, and AI Token quota management.

* **Entities:**
* `AiPackage` — Core AI subscription/top-up packages. Fields: `id`, `name`, `type` (enum: `subscription`, `top_up`), `price` (decimal), `token_quota` (int), `is_active` (boolean).
* `OrganizerAiQuota` — AI token wallet/quota for organizers. Fields: `id`, `organizer_id` (Logical FK to User, Unique), `subscription_tokens` (int), `top_up_tokens` (int), `updated_at`.
* `AiTokenTransaction` — Ledger for token usage and top-ups. Fields: `id`, `quota_id` (FK to OrganizerAiQuota), `package_id` (FK to AiPackage, NULLABLE), `type` (enum: `top_up`, `monthly_grant`, `usage_post`, `usage_segment`, `expired`), `amount` (int), `balance_after` (int), `reference_id` (Logical FK to Post or Payment, NULLABLE), `created_at`.
* `MarketingContent` — Nội dung do AI sinh ra. Fields: `title`, `lang_code`, `publisher_id`, `clicks`, `description`, `tags`, `performance_metrics` (TEXT), `status` (enum: `draft`, `pending`, `published`, `closed`), `event_id`, `ai_tokens_used` (int).
* `MarketingAnalytics` — Fields: `marketing_content_id`, `platform` (enum: `fb`, `web`, `zalo`), `date`, `impressions`, `clicks`, `conversion`, `click_through_rate`, `conversion_rate`, `revenue_generated`, `event_id`.
* `UserInterestScores` — Fields: `user_id`, `category_id`, `interest_score` (float), `last_interaction_at`.
* `MarketingAudienceSegments` — Fields: `segment_name`, `criteria` (JSONB), `generated_by_ai` (boolean), `created_at`.
* `UserPrompt` — Prompt do organizer nhập để AI sinh content. Fields: `title`, `content` (TEXT), `user_id`, `description`, `event_id`.
* `UserBehaviorLogs` — Log hành vi user để feed vào scoring/segmentation. Fields: `session_id`, `user_id`, `event_id`, `action_type` (ENUM), `metadata` (JSON), `created_at`.

* **AI Content Generation Flow:**
1. Organizer nhập prompt ngắn vào `UserPrompt` (mô tả event, loại content cần).
2. System kiểm tra `OrganizerAiQuota` đảm bảo đủ token khả dụng.
3. System gọi AI API với prompt + context event.
4. Trừ token trong `OrganizerAiQuota` (ưu tiên `subscription_tokens` trước) và ghi log vào `AiTokenTransaction`.
5. Kết quả được lưu vào `MarketingContent` (với `ai_tokens_used`) với `status = draft`.
6. Organizer review → publish hoặc chỉnh sửa.

* **Audience Segmentation Flow:**
1. `UserBehaviorLogs` ghi lại các hành động: view event, click, purchase, search...
2. Background job tính toán `UserInterestScores` theo category.
3. AI hoặc rule-based engine tạo `MarketingAudienceSegments` dựa trên `criteria` JSONB.

---

## 5. Coding Standards & Rules (Strict Enforcement)

### General
* **SOLID:** Adhere strictly to SOLID principles.
* **Naming:** PascalCase for Classes/Methods, camelCase for variables/parameters.
* **Async:** Use `async/await` for all I/O operations.
* **Error Handling:** Use Result Pattern — never throw exceptions for business logic errors.

### .NET 9 Specifics
* Use **Primary Constructors** for dependency injection.
* Use `collection expressions []`.
* Minimal API for all endpoint definitions.

### Module Isolation Rules
* Each module has its **own DbContext** and **PostgreSQL Schema**.
* **NO cross-module DbContext queries.** If Module A needs data from Module B, it must:
* Call a **public interface** (sync, intra-process), OR
* Consume an **Integration Event** via Outbox/Inbox pattern (async).
* Integration Events are the preferred pattern for state changes that affect multiple modules (e.g., `OrderPaidIntegrationEvent`, `EventPublishedIntegrationEvent`).

---

## 6. Key Cross-Module Integration Events

| Event | Publisher | Subscribers | Purpose |
|---|---|---|---|
| `OrderPaidIntegrationEvent` | Ticketing | — | Trigger QR generation, send confirmation email |
| `EventPublishedIntegrationEvent` | Event | Marketing | Start tracking, enable segmentation |
| `OrderCancelledIntegrationEvent` | Ticketing | Payment | Trigger refund flow if applicable |
| `PaymentSuccessIntegrationEvent` | Payment | Ticketing, Marketing | Confirm order → issue tickets, OR allocate AI tokens if AiPackage was purchased |

---

## 7. Changelog
| Version | Date | Changes |
|---|---|---|
| v1.1 | 2025 | `Seat.status` removed — availability tính động qua OrderTicket + Redis |
| v1.1 | 2025 | `Order.type` field added — enum values TBD |
| v1.1 | 2025 | `Event.spec` (JSONB) field added |
| v1.1 | 2025 | `Event.suspended_by` (GUID NULLABLE) field added |
| v1.1 | 2025 | `EventCategory.location` field removed |
| v1.1 | 2025 | `Area.row_table` field removed |
| v1.1 | 2025 | `Hashtag` và `EventHashtag` moved under Event module explicitly |
| v1.2 | 2026 | Added `AiPackage`, `OrganizerAiQuota`, `AiTokenTransaction` for AI Token/Billing management; updated cross-module events |

---

## 8. Instruction for AI Assistant
* **Role:** Act as a Senior Software Architect and Lead Developer.
* **Task:** When asked to write code, always check which **Module** the feature belongs to.
* **Constraint:** If a feature requires data from another module (e.g., *Create Order* needs *Event* data), simulate a check via an Interface or Integration Event — do **not** query the other module's DbContext directly.
* **Output:** Provide the file path/structure for every code snippet generated.
* Example: `src/Modules/Event/Application/Commands/CreateEvent/CreateEventCommandHandler.cs`
* **Validation:** Before implementing any business logic, re-read the relevant Business Rules section in this document.
