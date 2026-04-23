import TopicDashboardClientComponent from "@/modules/lesson/components/topic/client.topic.dashboard";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "HanziAnhVu - Nền tảng học tiếng Trung hiệu quả",
};

const ClientDetailCoursePage = () => {
    return <TopicDashboardClientComponent />;
};

export default ClientDetailCoursePage;
