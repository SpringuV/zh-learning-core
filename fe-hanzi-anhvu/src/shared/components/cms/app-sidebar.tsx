"use client";

import * as React from "react";

import { NavMain } from "@/shared/components/cms/nav-main";
import { NavUser } from "@/shared/components/cms/nav-user";
import { TeamSwitcher } from "@/shared/components/cms/team-switcher";
import {
    Sidebar,
    SidebarContent,
    SidebarFooter,
    SidebarHeader,
    SidebarRail,
} from "@/shared/components/ui/sidebar";
import {
    GalleryVerticalEndIcon,
    BookOpenIcon,
    Settings2Icon,
    FrameIcon,
    PieChartIcon,
    MapIcon,
} from "lucide-react";
import { useSession } from "next-auth/react";

// This is sample data.
const data = {
    teams: [
        {
            name: "HanZi Admin",
            logo: <GalleryVerticalEndIcon />,
            plan: "Enterprise",
        },
    ],
    navMain: [
        {
            title: "Bảng Điều Khiển",
            url: "/cms/dashboard",
            icon: <PieChartIcon />,
            isActive: true,
        },
        {
            title: "Quản Lý Người Dùng",
            url: "#",
            icon: <GalleryVerticalEndIcon />,
            items: [
                {
                    title: "Danh Sách User",
                    url: "/cms/users",
                },
                {
                    title: "Vai Trò & Quyền",
                    url: "/cms/roles",
                },
                {
                    title: "Hoạt Động User",
                    url: "/cms/user-activity",
                },
            ],
        },
        {
            title: "Quản Lý Nội Dung",
            url: "#",
            icon: <BookOpenIcon />,
            items: [
                {
                    title: "Khóa học công khai",
                    url: "/cms/lessons",
                },
            ],
        },
        {
            title: "Lớp Học và Học Viên",
            url: "#",
            icon: <FrameIcon />,
            items: [
                {
                    title: "Lớp học trực tuyến",
                    url: "/cms/classrooms",
                },
                {
                    title: "Quản Lý Học Viên",
                    url: "/cms/classrooms/students",
                },
            ],
        },
        {
            title: "Thanh Toán & Đơn Hàng",
            url: "#",
            icon: <PieChartIcon />,
            items: [
                {
                    title: "Danh Sách Đơn Hàng",
                    url: "/cms/orders",
                },
                {
                    title: "Gói Học",
                    url: "/cms/packages",
                },
                {
                    title: "Báo Cáo Doanh Thu",
                    url: "/cms/revenue-reports",
                },
            ],
        },
        {
            title: "Phân Tích & Báo Cáo",
            url: "#",
            icon: <MapIcon />,
            items: [
                {
                    title: "Thống Kê Tổng Quát",
                    url: "/cms/analytics",
                },
                {
                    title: "Báo Cáo User",
                    url: "/cms/user-reports",
                },
                {
                    title: "Báo Cáo Doanh Thu",
                    url: "/cms/financial-reports",
                },
            ],
        },
        {
            title: "Cài Đặt",
            url: "#",
            icon: <Settings2Icon />,
            items: [
                {
                    title: "Cài Đặt Chung",
                    url: "/cms/settings/general",
                },
                {
                    title: "Mẫu Email",
                    url: "/cms/settings/email-templates",
                },
                {
                    title: "Cấu Hình Thanh Toán",
                    url: "/cms/settings/payment-config",
                },
                {
                    title: "Hệ Thống",
                    url: "/cms/settings/system",
                },
            ],
        },
    ],
    projects: [],
};

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
    // user: {
    //         name: "shadcn",
    //         email: "m@example.com",
    //         avatar: "/avatars/shadcn.jpg",
    //     },
    const { data: session } = useSession();
    const user = {
        name: "Admin User",
        email: session?.user.name,
        avatar: "/avatars/shadcn.jpg",
    };
    return (
        <Sidebar collapsible="icon" {...props}>
            <SidebarHeader>
                <TeamSwitcher teams={data.teams} />
            </SidebarHeader>
            <SidebarContent>
                <NavMain items={data.navMain} />
            </SidebarContent>
            <SidebarFooter>
                <NavUser user={user} />
            </SidebarFooter>
            <SidebarRail />
        </Sidebar>
    );
}

/*
--------- NOTE  Hoạt động User---------
Chức Năng:
    Tracking user actions - Xem user đã làm gì
    Security monitoring - Phát hiện hoạt động bất thường, login từ nơi غريب
    Audit log - Ghi lại lịch sử để phục vụ kiểm toán
    User behavior analysis - Hiểu user dùng app như thế nào
Loại Hoạt Động (Activity Types):
    🔐 Authentication - Login, logout, đăng ký
    📚 Learning - Xem bài, làm bài, hoàn thành lesson
    💰 Payment - Mua gói, thanh toán
    ⚙️ Settings - Thay đổi profile, cài đặt
    📥 Downloads - Tải tài liệu
    🎯 Other - Bình luận, yêu thích, etc.
Thông Tin Cơ Bản:
    User Activity Table:
    ├── User Name (tên người dùng)
    ├── Email
    ├── Activity Type (Login, View Lesson, Submit Quiz, etc.)
    ├── Description (Chi tiết hành động)
    ├── Timestamp (Thời gian)
    ├── Duration (Khoảng thời gian nó diễn ra)
    ├── IP Address (Địa chỉ IP)
    ├── Device (Browser, OS)
    └── Status (Success, Failed)
Features Có Thể Thêm:
    🔍 Search & Filter - Tìm theo user, loại activity, khoảng thời gian
    📊 Analytics - Thống kê hoạt động: most active users, peak hours
    🚨 Alerts - Cảnh báo hoạt động đáng ngờ
    📥 Export - Xuất CSV/Excel cho báo cáo

*/
