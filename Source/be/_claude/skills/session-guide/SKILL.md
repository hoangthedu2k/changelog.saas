# Skill: Session Guide — ChangelogSaaS

## Khi nào dùng skill này
Khi user nói: "bắt đầu buổi [X] tuần [Y]", "hướng dẫn buổi hôm nay", "tiếp tục từ chỗ dừng", hoặc bất kỳ câu nào bắt đầu một coding session mới.

## Quy trình thực hiện

### Bước 1 — Đọc kế hoạch buổi hôm nay
Đọc file `C:\MyWork\Personal\ChangeLogSaaS\Docs\weekly_build_plan.html` để tìm tasks của tuần/buổi mà user đề cập.

Mapping buổi → ngày trong tuần:
- Buổi 1 = Thứ 2
- Buổi 2 = Thứ 3
- Buổi 3 = Thứ 4
- Buổi 4 = Thứ 5
- Buổi 5 = Thứ 6

### Bước 2 — Scan code hiện tại
Tùy tuần, scan các file liên quan:
- Tuần 1–2: `Source/be/**/*.cs` (exclude obj/, .vs/)
- Tuần 3–4: `Source/fe/**/*.ts`, `Source/fe/**/*.html`
- Tuần 5: Stripe/Email integration files
- Tuần 6: CI/CD, appsettings production

### Bước 3 — Review nhanh trạng thái
So sánh tasks trong kế hoạch vs code thực tế:
- Tasks nào đã done (có code implement)?
- Tasks nào còn thiếu hoặc skeleton rỗng?
- Có issue nào từ buổi trước chưa fix?

### Bước 4 — Tạo Task List cho buổi hôm nay
Dùng TaskCreate để tạo checklist cụ thể cho buổi này. Mỗi task = 1 deliverable rõ ràng (file/feature/test).

### Bước 5 — Implement từng task
Với mỗi task:
1. Giải thích ngắn gọn sẽ làm gì
2. Viết code trực tiếp vào file (dùng Edit/Write tools)
3. Mark task completed
4. Chuyển task tiếp theo

### Bước 6 — Tổng kết cuối buổi
Sau khi xong tất cả tasks:
- List những gì đã hoàn thành
- Note issues/debt cho buổi sau
- Milestone check: đã đủ điều kiện milestone tuần này chưa?

## Conventions luôn áp dụng (từ memory project_conventions)
- DateTime.UtcNow (không bao giờ DateTime.Now)
- Factory methods cho entities
- Private setters
- public interfaces (không internal)
- Minimal API style cho endpoints
- Tiếng Việt khi giải thích, English trong code

## Context quan trọng
- Solo developer, 10–20h/tuần → pragmatic, không over-engineer
- Stack: .NET 10, EF Core 9, PostgreSQL, Redis, Hangfire, Angular standalone SSR
- Target: ship được trong 6 tuần, charge khách tháng 7/2026
