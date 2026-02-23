# Tham khảo tài liệu về tùy chỉnh container cho Visual Studio
# https://aka.ms/customizecontainer


# Stage base: Dùng khi chạy debug nhanh từ Visual Studio
 # Image runtime ASP.NET Core 10.0 để chạy ứng dụng trong môi trường production hoặc debug nhanh từ Visual Studio
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base 
# Chạy container với user bảo mật thay vì root để tăng cường bảo mật. 
# APP_UID là biến môi trường được định nghĩa trong image base, thường có giá trị là 1000 hoặc 1001 tùy thuộc vào image cụ thể. 
# Việc sử dụng user này giúp giảm nguy cơ bảo mật nếu container bị tấn công, 
# vì attacker sẽ không có quyền root để thực hiện các hành động nguy hiểm trên hệ thống.
USER $APP_UID     
 # Đặt thư mục làm việc mặc định trong container là /app, nơi ứng dụng sẽ được chạy. 
 # Điều này giúp tổ chức file và thư mục trong container một cách rõ ràng và dễ quản lý.                                 
WORKDIR /app                 
# Mở cổng 8080 cho ứng dụng ASP.NET Core để lắng nghe các yêu cầu HTTP.
# Cổng này thường được sử dụng cho ứng dụng web, và việc mở cổng này cho phép container nhận và xử lý các yêu cầu từ bên ngoài
# khi chạy trong môi trường production hoặc debug nhanh từ Visual Studio.                     
EXPOSE 8080     
 # Mở thêm cổng 8081 để hỗ trợ debug nhanh từ Visual Studio, cho phép Visual Studio kết nối đến container
 # để gỡ lỗi ứng dụng một cách hiệu quả. Việc mở cổng này giúp cải thiện trải nghiệm gỡ lỗi khi phát triển ứng dụng trong môi trường container.                                   
EXPOSE 8081                                       



# Stage build: Dùng để build project
# Image SDK .NET 10.0 để build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build      
# Biến cấu hình build (Release/Debug)   
ARG BUILD_CONFIGURATION=Release       
# Đặt thư mục làm việc là /src để chuẩn bị copy mã nguồn vào container                  
WORKDIR /src  
# Copy file project vào container                                          
COPY ["hanzi-anhvu-hsk.csproj", "."]  
# Khôi phục các package NuGet cần thiết cho project để chuẩn bị cho quá trình build                
RUN dotnet restore "./hanzi-anhvu-hsk.csproj"        
 # Copy toàn bộ mã nguồn vào container  
COPY . .                      
 # Đặt lại thư mục làm việc (tương tự /src)                         
WORKDIR "/src/."                              
# Build project với cấu hình đã chỉ định và xuất ra thư mục /app/build trong container để chuẩn bị cho stage publish.        
RUN dotnet build "./hanzi-anhvu-hsk.csproj" -c $BUILD_CONFIGURATION -o /app/build  


# Stage publish: Đóng gói ứng dụng đã build để chạy production
# Tạo stage mới từ kết quả build
FROM build AS publish                           
# Biến cấu hình build (Release/Debug)        
ARG BUILD_CONFIGURATION=Release      
# Publish project với cấu hình đã chỉ định và xuất ra thư mục /app/publish trong container. Tham số /p:UseAppHost=false được sử dụng để không tạo file thực thi riêng cho ứng dụng, giúp giảm kích thước của image cuối cùng.                   
RUN dotnet publish "./hanzi-anhvu-hsk.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false  


# Stage final: Dùng để chạy ứng dụng (production hoặc VS mode thường)
# Stage cuối cùng từ base (runtime)
FROM base AS final 
# Đặt thư mục làm việc là /app để chuẩn bị chạy ứng dụng                                    
WORKDIR /app                   
# Copy file đã publish vào thư mục hiện tại của container (từ stage publish) để chuẩn bị chạy ứng dụng.                        
COPY --from=publish /app/publish .     
# Lệnh mặc định khi container khởi động, chạy ứng dụng bằng lệnh dotnet với file DLL của ứng dụng.                
ENTRYPOINT ["dotnet", "hanzi-anhvu-hsk.dll"]           