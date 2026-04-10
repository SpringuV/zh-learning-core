"use client";

import { useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { Badge } from "@/shared/components/ui/badge";

// Mock Data
const mockCourse = {
    id: "1",
    title: "HSK 1 - Nhập môn",
    description:
        "Basic Chinese characters and phrases for absolute beginners. Covers daily greetings, numbers, and simple sentence structures.",
    level: 1,
    published: true,
    totalStudents: 234,
    createdAt: "2025-01-10",
};

const mockTopics = [
    {
        id: "t1",
        title: "Xin chào (Greetings)",
        orderIndex: 1,
        exercisesCount: 5,
        status: "published",
    },
    {
        id: "t2",
        title: "Số đếm (Numbers 1-100)",
        orderIndex: 2,
        exercisesCount: 8,
        status: "published",
    },
    {
        id: "t3",
        title: "Gia đình (Family)",
        orderIndex: 3,
        exercisesCount: 4,
        status: "draft",
    },
];

export default function CourseDetailsPage() {
    const params = useParams();
    const courseId = params["course-id"];

    const [activeTab, setActiveTab] = useState<"topics" | "settings">("topics");

    return (
        <div className="-mx-6 -mt-6 min-h-screen bg-linear-to-br from-slate-50 via-white to-slate-50">
            {/* Header Section */}
            <div className="pt-8 px-8 border-b border-slate-200/50 bg-white/80 backdrop-blur-xl">
                <div className="flex flex-col md:flex-row justify-between gap-6 items-start md:items-center">
                    <div>
                        <div className="flex items-center gap-3 mb-2">
                            <Badge
                                variant="outline"
                                className="bg-amber-50 text-amber-700 border-amber-200"
                            >
                                HSK {mockCourse.level}
                            </Badge>
                            <Badge
                                variant="secondary"
                                className={
                                    mockCourse.published
                                        ? "bg-emerald-100 text-emerald-700"
                                        : "bg-slate-100 text-slate-700"
                                }
                            >
                                {mockCourse.published
                                    ? "Đang xuất bản"
                                    : "Bản nháp"}
                            </Badge>
                        </div>
                        <h1 className="text-3xl font-semibold text-slate-900 tracking-tight">
                            {mockCourse.title}
                        </h1>
                        <p className="text-slate-500 mt-2 max-w-2xl text-sm">
                            {mockCourse.description}
                        </p>
                    </div>

                    <div className="flex gap-3">
                        <Link href="/cms/lessons/course">
                            <button className="px-5 py-2.5 bg-white border border-slate-200 text-slate-700 rounded-lg hover:bg-slate-50 transition-colors font-medium text-sm">
                                Quay lại
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
                        { id: "topics", label: "Quản lý Topics" },
                        { id: "settings", label: "Cài đặt khóa học" },
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
                {activeTab === "topics" && (
                    <div className="space-y-6">
                        <div className="flex justify-between items-center bg-white p-5 rounded-xl border border-slate-200/50 shadow-sm">
                            <div>
                                <h2 className="text-lg font-semibold text-slate-900">
                                    Nội dung bài học (Topics)
                                </h2>
                                <p className="text-sm text-slate-500">
                                    Quản lý các chương/chủ đề trong khóa học này
                                </p>
                            </div>
                            <button className="px-4 py-2 bg-slate-900 text-white rounded-lg text-sm font-medium hover:bg-slate-800 transition-colors">
                                + Thêm Topic mới
                            </button>
                        </div>

                        <div className="bg-white rounded-xl border border-slate-200/50 shadow-sm overflow-hidden">
                            <table className="w-full text-sm text-left">
                                <thead className="bg-slate-50/50 border-b border-slate-200/50">
                                    <tr>
                                        <th className="px-6 py-4 font-semibold text-slate-600 w-16 text-center">
                                            STT
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Tên Topic
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Số bài tập
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
                                    {mockTopics.map((topic) => (
                                        <tr
                                            key={topic.id}
                                            className="border-b border-slate-50 hover:bg-slate-50/50 transition-colors group"
                                        >
                                            <td className="px-6 py-4 text-center font-medium text-slate-400">
                                                #{topic.orderIndex}
                                            </td>
                                            <td className="px-6 py-4 font-medium text-slate-900">
                                                {topic.title}
                                            </td>
                                            <td className="px-6 py-4 text-slate-500">
                                                {topic.exercisesCount} bài tập
                                            </td>
                                            <td className="px-6 py-4">
                                                <Badge
                                                    variant="outline"
                                                    className={
                                                        topic.status ===
                                                        "published"
                                                            ? "bg-emerald-50 text-emerald-600 border-emerald-200"
                                                            : "bg-slate-100 text-slate-600 border-slate-200"
                                                    }
                                                >
                                                    {topic.status ===
                                                    "published"
                                                        ? "Live"
                                                        : "Draft"}
                                                </Badge>
                                            </td>
                                            <td className="px-6 py-4 text-right">
                                                {/* Link đi vào màn hình Topic Detail (quản lý Exercise) */}
                                                <Link
                                                    href={`/cms/lessons/course/${courseId}/topics/${topic.id}`}
                                                >
                                                    <button className="px-3 py-1.5 text-xs bg-white border border-slate-200 text-slate-600 rounded hover:bg-slate-50 hover:text-amber-600 transition-colors shadow-sm">
                                                        Quản lý bài tập
                                                    </button>
                                                </Link>
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
                            Thông tin chung
                        </h2>

                        <div className="space-y-6">
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-2">
                                    Tên khóa học
                                </label>
                                <input
                                    type="text"
                                    defaultValue={mockCourse.title}
                                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all"
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-2">
                                    Mô tả (Description)
                                </label>
                                <textarea
                                    rows={4}
                                    defaultValue={mockCourse.description}
                                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all resize-none"
                                ></textarea>
                            </div>

                            <div className="grid grid-cols-2 gap-6">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-2">
                                        Cấp độ (HSK Level)
                                    </label>
                                    <select
                                        defaultValue={mockCourse.level}
                                        className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all"
                                    >
                                        <option value={1}>HSK 1</option>
                                        <option value={2}>HSK 2</option>
                                        <option value={3}>HSK 3</option>
                                        <option value={4}>HSK 4</option>
                                        <option value={5}>HSK 5</option>
                                        <option value={6}>HSK 6</option>
                                    </select>
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-2">
                                        Trạng thái xuất bản
                                    </label>
                                    <div className="flex items-center h-10 px-4 bg-slate-50 border border-slate-200 rounded-lg">
                                        <label className="flex items-center gap-2 cursor-pointer">
                                            <input
                                                type="checkbox"
                                                defaultChecked={
                                                    mockCourse.published
                                                }
                                                className="w-4 h-4 text-amber-600 rounded focus:ring-amber-500"
                                            />
                                            <span className="text-sm text-slate-700">
                                                Công khai (Public)
                                            </span>
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}
