outbox pattern

Để “chuẩn production” hơn, thêm 4 điểm:
1.	Notification xử lý idempotent theo MessageId/EventId
2.	Có retry + dead-letter (hoặc max retry) ở consumer
3.	Event contract versioning (tránh breaking khi đổi payload)
4.	Chỉ mark processed khi xử lý thành công
Nếu bạn đang làm như trên thì kiến trúc là đúng hướng.