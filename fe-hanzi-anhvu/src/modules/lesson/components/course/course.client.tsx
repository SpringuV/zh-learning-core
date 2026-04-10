"use client";

import { useState } from "react";
import Link from "next/link";
import { PenSquare, FolderOpen } from "lucide-react";
import {
    CreateCourseModal,
    type CreateCoursePayload,
} from "@/modules/lesson/components/course/create-course-modal";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Badge } from "@/shared/components/ui/badge";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/components/ui/select";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/shared/components/ui/table";
import {
    Tooltip,
    TooltipContent,
    TooltipTrigger,
} from "@/shared/components/ui/tooltip";

// Mock data - replace with real API
const initialCourses = [
    {
        id: "1",
        title: "HSK 1 - Nhap mon",
        slug: "hsk-1-starter",
        description: "Ky tu va cum tu tieng Trung co ban",
        level: 1,
        orderIndex: 1,
        topics: 12,
        students: 234,
        published: true,
        updatedAt: "2025-04-05",
    },
    {
        id: "2",
        title: "HSK 2 - Co ban",
        slug: "hsk-2-elementary",
        description: "Hoi thoai hang ngay pho bien",
        level: 2,
        orderIndex: 2,
        topics: 15,
        students: 156,
        published: true,
        updatedAt: "2025-04-03",
    },
    {
        id: "3",
        title: "HSK 3 - Trung cap",
        slug: "hsk-3-intermediate",
        description: "Cau truc cau phuc tap hon",
        level: 3,
        orderIndex: 3,
        topics: 18,
        students: 89,
        published: false,
        updatedAt: "2025-04-01",
    },
];

export default function CourseCmsPage() {
    const [courses, setCourses] = useState(initialCourses);
    const [activeTab, setActiveTab] = useState<"courses" | "stats">("courses");
    const [searchTerm, setSearchTerm] = useState("");
    const [levelFilter, setLevelFilter] = useState("all");

    const totalCourses = courses.length;
    const publishedCourses = courses.filter(
        (course) => course.published,
    ).length;
    const draftCourses = totalCourses - publishedCourses;
    const totalTopics = courses.reduce((sum, course) => sum + course.topics, 0);
    const totalStudents = courses.reduce(
        (sum, course) => sum + course.students,
        0,
    );

    const filteredCourses = courses.filter((course) => {
        const keyword = searchTerm.trim().toLowerCase();
        const matchesKeyword =
            keyword.length === 0 ||
            course.title.toLowerCase().includes(keyword) ||
            course.description.toLowerCase().includes(keyword) ||
            course.slug.toLowerCase().includes(keyword);

        const matchesLevel =
            levelFilter === "all" || String(course.level) === levelFilter;

        return matchesKeyword && matchesLevel;
    });

    const formatDate = (value: string) => {
        return new Date(value).toLocaleDateString("vi-VN", {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
        });
    };

    const handleCreateCourse = (payload: CreateCoursePayload) => {
        setCourses((prev) => {
            const newCourse = {
                id: String(Date.now()),
                title: payload.title,
                slug: payload.slug,
                description: payload.description || "Chưa có mô tả",
                level: payload.hskLevel,
                orderIndex: payload.orderIndex,
                topics: 0,
                students: 0,
                published: false,
                updatedAt: new Date().toISOString().slice(0, 10),
            };

            return [...prev, newCourse].sort(
                (a, b) => a.orderIndex - b.orderIndex,
            );
        });
    };

    return (
        <div className="bg-linear-to-br from-slate-50 via-white to-slate-50">
            {/* Top Navigation Bar */}
            <div className="sticky top-0 z-40 border-b border-slate-200/50 bg-white/80 backdrop-blur-lg">
                <div className="max-w-[80vw] mx-auto px-5 py-4">
                    <div className="flex items-center justify-between">
                        <div>
                            <h1 className="text-3xl font-light tracking-tight">
                                <span className="text-slate-900">Quản lý</span>
                                <span className="text-amber-600">
                                    {" "}
                                    khóa học
                                </span>
                            </h1>
                            <p className="text-slate-500 text-sm mt-0.5">
                                Tạo và quản lý khóa học, chủ đề và bài tập
                            </p>
                        </div>
                        <CreateCourseModal
                            defaultOrderIndex={courses.length + 1}
                            onCreate={handleCreateCourse}
                        />
                    </div>
                </div>
            </div>

            {/* Main Content */}
            <div className="max-w-7xl mx-auto px-5 py-6">
                {/* Tabs */}
                <div className="mb-5 flex items-center justify-between">
                    <div className="flex gap-1 bg-slate-100/50 p-1 rounded-lg w-fit">
                        {[
                            {
                                id: "courses",
                                label: "Khóa học",
                                count: courses.length,
                            },
                            { id: "stats", label: "Thống kê", count: null },
                        ].map((tab) => (
                            <Button
                                type="button"
                                key={tab.id}
                                onClick={() => setActiveTab(tab.id as any)}
                                variant={
                                    activeTab === tab.id ? "secondary" : "ghost"
                                }
                                size="sm"
                                className={`rounded-md font-medium transition-all duration-300 text-sm ${
                                    activeTab === tab.id
                                        ? "bg-white text-slate-900 shadow-sm"
                                        : "text-slate-600 hover:text-slate-900"
                                }`}
                            >
                                {tab.label}
                                {tab.count !== null && (
                                    <span className="ml-2 text-xs bg-slate-200 px-2 py-1 rounded">
                                        {tab.count}
                                    </span>
                                )}
                            </Button>
                        ))}
                    </div>
                    <div className="flex items-center justify-between mb-4">
                        <div className="flex gap-3">
                            <Input
                                type="text"
                                value={searchTerm}
                                onChange={(event) =>
                                    setSearchTerm(event.target.value)
                                }
                                placeholder="Tìm kiếm khóa học..."
                                className="h-9 w-73 bg-white text-sm"
                            />
                            <Select
                                value={levelFilter}
                                onValueChange={setLevelFilter}
                            >
                                <SelectTrigger className="h-9 w-32.5 bg-white text-sm">
                                    <SelectValue placeholder="Tất cả" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">Tất cả</SelectItem>
                                    <SelectItem value="1">HSK 1</SelectItem>
                                    <SelectItem value="2">HSK 2</SelectItem>
                                    <SelectItem value="3">HSK 3</SelectItem>
                                    <SelectItem value="4">HSK 4</SelectItem>
                                    <SelectItem value="5">HSK 5</SelectItem>
                                    <SelectItem value="6">HSK 6</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                    </div>
                </div>

                {/* Courses Tab */}
                {activeTab === "courses" && (
                    <div>
                        {/* Table View */}
                        <div className="bg-white rounded-xl border border-slate-200/50 overflow-hidden shadow-sm">
                            <Table>
                                <TableHeader className="bg-slate-50/50">
                                    <TableRow className="border-b border-slate-200/50 hover:bg-slate-50/50">
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider w-16">
                                            #
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Tên khóa học
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Slug
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Cấp độ
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Chủ đề
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Học viên
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Cập nhật
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Trạng thái
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Thao tác
                                        </TableHead>
                                    </TableRow>
                                </TableHeader>

                                <TableBody>
                                    {filteredCourses.map((course) => (
                                        <TableRow
                                            key={course.id}
                                            className="border-b border-slate-100 hover:bg-slate-50/50"
                                        >
                                            <TableCell className="px-4 py-3 text-sm font-medium text-slate-500">
                                                {course.orderIndex}
                                            </TableCell>
                                            <TableCell className="px-4 py-3 whitespace-normal">
                                                <div>
                                                    <p className="font-medium text-slate-900 text-sm">
                                                        {course.title}
                                                    </p>
                                                    <p className="text-xs text-slate-500 mt-0.5">
                                                        {course.description}
                                                    </p>
                                                </div>
                                            </TableCell>
                                            <TableCell className="px-4 py-3 text-xs text-slate-500 font-mono">
                                                {course.slug}
                                            </TableCell>
                                            <TableCell className="px-4 py-3">
                                                <Badge className="bg-amber-100 text-amber-700">
                                                    HSK {course.level}
                                                </Badge>
                                            </TableCell>
                                            <TableCell className="px-4 py-3 text-sm text-slate-600">
                                                {course.topics}
                                            </TableCell>
                                            <TableCell className="px-4 py-3 text-sm text-slate-600">
                                                {course.students}
                                            </TableCell>
                                            <TableCell className="px-4 py-3 text-xs text-slate-500">
                                                {formatDate(course.updatedAt)}
                                            </TableCell>
                                            <TableCell className="px-4 py-3">
                                                <Badge
                                                    className={`${
                                                        course.published
                                                            ? "bg-green-100 text-green-700"
                                                            : "bg-slate-100 text-slate-600"
                                                    }`}
                                                >
                                                    {course.published
                                                        ? "Đã xuất bản"
                                                        : "Nháp"}
                                                </Badge>
                                            </TableCell>
                                            <TableCell className="px-4 py-3">
                                                <div className="flex gap-2">
                                                    <Tooltip>
                                                        <TooltipTrigger asChild>
                                                            <Button
                                                                asChild
                                                                type="button"
                                                                size="icon-sm"
                                                                variant="outline"
                                                                className="text-slate-700 hover:text-slate-900"
                                                            >
                                                                <Link
                                                                    href={`/cms/lessons/course/${course.id}?tab=settings`}
                                                                >
                                                                    <PenSquare className="h-4 w-4" />
                                                                </Link>
                                                            </Button>
                                                        </TooltipTrigger>
                                                        <TooltipContent>
                                                            Sửa khóa học
                                                        </TooltipContent>
                                                    </Tooltip>

                                                    <Tooltip>
                                                        <TooltipTrigger asChild>
                                                            <Button
                                                                asChild
                                                                type="button"
                                                                size="icon-sm"
                                                                variant="outline"
                                                                className="text-slate-700 hover:text-slate-900"
                                                            >
                                                                <Link
                                                                    href={`/cms/lessons/course/${course.id}`}
                                                                >
                                                                    <FolderOpen className="h-4 w-4" />
                                                                </Link>
                                                            </Button>
                                                        </TooltipTrigger>
                                                        <TooltipContent>
                                                            Vào chi tiết khóa
                                                        </TooltipContent>
                                                    </Tooltip>
                                                </div>
                                            </TableCell>
                                        </TableRow>
                                    ))}

                                    {filteredCourses.length === 0 && (
                                        <TableRow className="hover:bg-transparent">
                                            <TableCell
                                                colSpan={9}
                                                className="h-24 text-center text-sm text-slate-500"
                                            >
                                                Không có khóa học phù hợp với bộ
                                                lọc hiện tại.
                                            </TableCell>
                                        </TableRow>
                                    )}
                                </TableBody>
                            </Table>
                        </div>
                    </div>
                )}

                {/* Statistics Tab */}
                {activeTab === "stats" && (
                    <div className="space-y-4">
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                            {[
                                {
                                    label: "Tổng khóa học",
                                    value: String(totalCourses),
                                    tone: "text-slate-900",
                                },
                                {
                                    label: "Đã xuất bản",
                                    value: String(publishedCourses),
                                    tone: "text-emerald-600",
                                },
                                {
                                    label: "Bản nháp",
                                    value: String(draftCourses),
                                    tone: "text-slate-700",
                                },
                                {
                                    label: "Tổng chủ đề",
                                    value: String(totalTopics),
                                    tone: "text-slate-900",
                                },
                                {
                                    label: "Tổng học viên",
                                    value: String(totalStudents),
                                    tone: "text-slate-900",
                                },
                            ].map((stat) => (
                                <div
                                    key={stat.label}
                                    className="bg-white rounded-xl border border-slate-200/50 p-5"
                                >
                                    <p className="text-sm text-slate-600 font-medium mb-2">
                                        {stat.label}
                                    </p>
                                    <p
                                        className={`text-3xl font-light mb-1 ${stat.tone}`}
                                    >
                                        {stat.value}
                                    </p>
                                </div>
                            ))}
                        </div>

                        <div className="bg-white rounded-xl border border-slate-200/50 p-5">
                            <p className="text-sm text-slate-600 font-medium mb-2">
                                Tỷ lệ xuất bản
                            </p>
                            <p className="text-2xl font-light text-slate-900">
                                {publishedCourses}/{totalCourses} khóa học đã
                                xuất bản
                            </p>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}
