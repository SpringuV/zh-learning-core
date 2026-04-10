import CourseCmsPage from "@/modules/lesson/components/course/course.client";
import { Metadata } from "next";
export const metadata: Metadata = {
    title: "Quản lý khóa học",
    description: "Quản lý danh sách khóa học trong hệ thống",
};

export default function CoursePage() {
    return <CourseCmsPage />;
}
