Integration event này sẽ là nơi chúng ta định nghĩa các sự kiện mà sẽ được publish ra ngoài qua event bus 
(ví dụ: RabbitMQ, Kafka, v.v.) để các hệ thống khác có thể lắng nghe và phản hồi.

- hiện tại trong project đang dùng event bus là inmemory event bus,
- nhưng sau này có thể sẽ chuyển sang một giải pháp khác như RabbitMQ hoặc Kafka, 
- nên cần có một lớp abstraction để dễ dàng thay đổi mà không ảnh hưởng đến phần còn lại của codebase.