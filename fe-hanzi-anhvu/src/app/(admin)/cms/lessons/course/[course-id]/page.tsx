import TopicManagementByCourse from "@/modules/lesson/components/topic/topic.management";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Quản lý chủ đề",
    description: "Quản lý danh sách chủ đề có trong khóa học",
};

const CourseDetailsPage = () => {
    return <TopicManagementByCourse />;
};

export default CourseDetailsPage;
