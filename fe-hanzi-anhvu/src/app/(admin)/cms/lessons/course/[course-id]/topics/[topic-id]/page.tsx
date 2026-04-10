"use client";

import { useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { Badge } from "@/shared/components/ui/badge";

// Mock Data
const mockTopic = {
    id: "t1",
    courseId: "1",
    courseTitle: "HSK 1 - Starter",
    title: "Xin chào (Greetings)",
    description:
        "Học cách chào hỏi cơ bản, làm quen, giới thiệu bản thân và giao tiếp những đoạn hội thoại đầu tiên bằng tiếng Trung.",
    orderIndex: 1,
    status: "published",
    createdAt: "2025-01-10",
};

const mockExercises = [
    {
        id: "e1",
        title: "Luyện nghe từ vựng",
        type: "listening",
        score: 10,
        status: "live",
    },
    {
        id: "e2",
        title: "Nhận diện chữ Hán",
        type: "reading",
        score: 15,
        status: "live",
    },
    {
        id: "e3",
        title: "Sắp xếp câu",
        type: "grammar",
        score: 20,
        status: "live",
    },
    {
        id: "e4",
        title: "Phát âm tiếng Trung",
        type: "speaking",
        score: 25,
        status: "draft",
    },
];

// Hàm phụ trợ để mapping loại bài tập thành label và màu đẹp
const exerciseTypeMap: Record<string, { label: string; color: string }> = {
    listening: {
        label: "Luyện nghe",
        color: "bg-blue-50 text-blue-700 border-blue-200",
    },
    reading: {
        label: "Luyện đọc",
        color: "bg-emerald-50 text-emerald-700 border-emerald-200",
    },
    grammar: {
        label: "Ngữ pháp",
        color: "bg-purple-50 text-purple-700 border-purple-200",
    },
    speaking: {
        label: "Phát âm",
        color: "bg-rose-50 text-rose-700 border-rose-200",
    },
};

export default function TopicDetailsPage() {
    const params = useParams();
    const courseId = params["course-id"];
    const topicId = params["topic-id"];

    const [activeTab, setActiveTab] = useState<"exercises" | "settings">(
        "exercises",
    );

    return (
        <div className="-mx-6 -mt-6 min-h-screen bg-gradient-to-br from-slate-50 via-white to-slate-50">
            {/* Header Section */}
            <div className="pt-8 px-8 border-b border-slate-200/50 bg-white/80 backdrop-blur-xl">
                <div className="flex flex-col md:flex-row justify-between gap-6 items-start md:items-center">
                    <div>
                        <div className="flex items-center gap-3 mb-2">
                            <Badge
                                variant="outline"
                                className="bg-slate-100 text-slate-600 border-slate-200"
                            >
                                Thuộc khóa: {mockTopic.courseTitle}
                            </Badge>
                            <Badge
                                variant="outline"
                                className="bg-amber-50 text-amber-700 border-amber-200"
                            >
                                Topic #{mockTopic.orderIndex}
                            </Badge>
                            <Badge
                                variant="secondary"
                                className={
                                    mockTopic.status === "published"
                                        ? "bg-emerald-100 text-emerald-700"
                                        : "bg-slate-100 text-slate-700"
                                }
                            >
                                {mockTopic.status === "published"
                                    ? "Đang xuất bản"
                                    : "Bản nháp"}
                            </Badge>
                        </div>
                        <h1 className="text-3xl font-semibold text-slate-900 tracking-tight">
                            {mockTopic.title}
                        </h1>
                        <p className="text-slate-500 mt-2 max-w-2xl text-sm">
                            {mockTopic.description}
                        </p>
                    </div>

                    <div className="flex gap-3 flex-shrink-0">
                        <Link href={`/cms/lessons/course/${courseId}`}>
                            <button className="px-5 py-2.5 bg-white border border-slate-200 text-slate-700 rounded-lg hover:bg-slate-50 transition-colors font-medium text-sm">
                                Về khóa học
                            </button>
                        </Link>
                        <button className="px-5 py-2.5 bg-amber-600 text-white rounded-lg hover:bg-amber-700 transition-colors font-medium text-sm">
                            Lưu thay đổi
                        </button>
                    </div>
                </div>

                {/* Tabs */}
                <div className="flex gap-8 pt-8">
                    {[
                        { id: "exercises", label: "Danh sách bài tập" },
                        { id: "settings", label: "Cài đặt & Thông tin" },
                    ].map((tab) => (
                        <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id as any)}
                            className={`pb-4 text-sm font-medium transition-colors relative ${
                                activeTab === tab.id
                                    ? "text-amber-600"
                                    : "text-slate-500 hover:text-slate-800"
                            }`}
                        >
                            {tab.label}
                            {activeTab === tab.id && (
                                <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-amber-600 z-10" />
                            )}
                        </button>
                    ))}
                </div>
            </div>

            {/* Tab Contents */}
            <div className="max-w-7xl mx-auto px-8 py-12">
                {activeTab === "exercises" && (
                    <div className="space-y-6">
                        <div className="flex justify-between items-center bg-white p-5 rounded-xl border border-slate-200/50 shadow-sm">
                            <div>
                                <h2 className="text-lg font-semibold text-slate-900">
                                    Danh sách bài tập (Exercises)
                                </h2>
                                <p className="text-sm text-slate-500">
                                    Học viên cần hoàn thành các bài tập này để
                                    vượt qua Topic
                                </p>
                            </div>
                            <div className="flex gap-3">
                                <button className="px-4 py-2 bg-slate-100 text-slate-700 rounded-lg text-sm font-medium hover:bg-slate-200 transition-colors">
                                    Thứ tự hiển thị
                                </button>
                                <button className="px-4 py-2 bg-slate-900 text-white rounded-lg text-sm font-medium hover:bg-slate-800 transition-colors shadow-sm">
                                    + Thêm Bài tập
                                </button>
                            </div>
                        </div>

                        <div className="bg-white rounded-xl border border-slate-200/50 shadow-sm overflow-hidden">
                            <table className="w-full text-sm text-left">
                                <thead className="bg-slate-50/50 border-b border-slate-200/50">
                                    <tr>
                                        <th className="px-6 py-4 font-semibold text-slate-600 w-16 text-center">
                                            ID
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Tiêu đề bài tập
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Loại kỹ năng
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Điểm tối đa
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Trạng thái
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600 text-right">
                                            Thao tác
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {mockExercises.map((exercise, index) => (
                                        <tr
                                            key={exercise.id}
                                            className="border-b border-slate-50 hover:bg-slate-50/50 transition-colors group"
                                        >
                                            <td className="px-6 py-4 text-center font-medium text-slate-400">
                                                {index + 1}
                                            </td>
                                            <td className="px-6 py-4 font-medium text-slate-900">
                                                {exercise.title}
                                            </td>
                                            <td className="px-6 py-4">
                                                <Badge
                                                    variant="outline"
                                                    className={
                                                        exerciseTypeMap[
                                                            exercise.type
                                                        ]?.color ||
                                                        "bg-slate-50 text-slate-600"
                                                    }
                                                >
                                                    {exerciseTypeMap[
                                                        exercise.type
                                                    ]?.label || "Khác"}
                                                </Badge>
                                            </td>
                                            <td className="px-6 py-4 text-slate-600 font-medium">
                                                {exercise.score} pt
                                            </td>
                                            <td className="px-6 py-4">
                                                <span
                                                    className={`inline-flex items-center gap-1.5 text-xs font-medium ${exercise.status === "live" ? "text-emerald-600" : "text-slate-500"}`}
                                                >
                                                    <span
                                                        className={`w-1.5 h-1.5 rounded-full ${exercise.status === "live" ? "bg-emerald-500" : "bg-slate-400"}`}
                                                    ></span>
                                                    {exercise.status === "live"
                                                        ? "Hiển thị"
                                                        : "Bản nháp"}
                                                </span>
                                            </td>
                                            <td className="px-6 py-4 text-right">
                                                <div className="flex justify-end gap-2">
                                                    <button className="px-3 py-1.5 text-xs text-slate-600 hover:bg-slate-100 rounded transition-colors font-medium">
                                                        Phân loại
                                                    </button>
                                                    <Link
                                                        href={`/cms/lessons/course/${courseId}/topics/${topicId}/exercises/${exercise.id}`}
                                                    >
                                                        <button className="px-3 py-1.5 text-xs bg-white border border-slate-200 text-slate-700 rounded hover:bg-slate-50 hover:text-amber-600 transition-colors shadow-[0_1px_2px_rgba(0,0,0,0.05)] font-medium">
                                                            Biên soạn
                                                        </button>
                                                    </Link>
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                )}

                {activeTab === "settings" && (
                    <div className="bg-white rounded-xl border border-slate-200/50 shadow-sm p-8 max-w-3xl">
                        <h2 className="text-xl font-semibold text-slate-900 mb-6">
                            Cài đặt Topic
                        </h2>

                        <div className="space-y-6">
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-2">
                                    Tên chủ đề (Topic Title)
                                </label>
                                <input
                                    type="text"
                                    defaultValue={mockTopic.title}
                                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all font-medium"
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-2">
                                    Giới thiệu ngắn (Description)
                                </label>
                                <textarea
                                    rows={3}
                                    defaultValue={mockTopic.description}
                                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all resize-none"
                                ></textarea>
                                <p className="text-xs text-slate-500 mt-1">
                                    Giúp học viên nắm được mục tiêu bài học.
                                </p>
                            </div>

                            <div className="grid grid-cols-2 gap-6">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-2">
                                        Thứ tự hiển thị (Order Index)
                                    </label>
                                    <input
                                        type="number"
                                        defaultValue={mockTopic.orderIndex}
                                        className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all"
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-2">
                                        Mở khóa nội dung
                                    </label>
                                    <div className="flex items-center h-10 px-4 bg-slate-50 border border-slate-200 rounded-lg">
                                        <label className="flex items-center gap-2 cursor-pointer">
                                            <input
                                                type="checkbox"
                                                defaultChecked={
                                                    mockTopic.status ===
                                                    "published"
                                                }
                                                className="w-4 h-4 text-amber-600 rounded focus:ring-amber-500"
                                            />
                                            <span className="text-sm text-slate-700">
                                                Topic đang kích hoạt
                                            </span>
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="mt-8 pt-6 border-t border-red-100">
                            <h3 className="text-sm font-medium text-red-600 mb-2">
                                Vùng nguy hiểm
                            </h3>
                            <button className="px-4 py-2 bg-red-50 text-red-600 border border-red-200 rounded-lg text-sm font-medium hover:bg-red-100 transition-colors">
                                Xóa hoàn toàn Topic này
                            </button>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}
