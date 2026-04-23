import { ClientDashboardMainContent } from "@/shared/components/clients/main.content";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "HanziAnhVu - Nền tảng học tiếng Trung hiệu quả",
    description: "Trang tổng quan của người dùng",
};

const DashboardClientPage = () => {
    return <ClientDashboardMainContent />;
};

export default DashboardClientPage;
