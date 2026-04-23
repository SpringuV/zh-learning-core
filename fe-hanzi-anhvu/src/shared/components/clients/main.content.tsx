"use client";
import { useGetCourseForDashboard } from "@/modules/lesson/hooks/use.course.tanstack";
import { CourseDashboardItem } from "@/modules/lesson/types/coure.type";
import { Sparkles } from "lucide-react";
import Link from "next/link";

export function ClientDashboardMainContent() {
    const courseDashboardList = useGetCourseForDashboard();
    if (courseDashboardList.isLoading || courseDashboardList.isFetching) {
        return (
            <div className="flex items-center justify-center h-full">
                <Sparkles className="animate-spin" size={48} />
            </div>
        );
    }
    if (courseDashboardList.isError) {
        return (
            <div className="flex items-center justify-center h-full">
                <p className="text-red-500">Có lỗi xảy ra.</p>
            </div>
        );
    }
    if (courseDashboardList.data?.data == null) {
        return (
            <div className="flex flex-col items-center justify-center h-full gap-4">
                <Sparkles size={48} className="text-slate-400" />
                <p className="text-slate-500">
                    Hiện tại không có khóa học nào.
                </p>
            </div>
        );
    }
    return (
        <section className="min-h-0 flex-1 w-full overflow-y-auto px-4 py-6 md:px-6">
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
                {courseDashboardList.data?.data.map(
                    (item: CourseDashboardItem, index: number) => (
                        <Link
                            key={index}
                            href={`/u/${item.slug}`}
                            className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition-shadow hover:shadow-md"
                        >
                            <h3 className="font-semibold text-slate-800">
                                {item.title}
                            </h3>
                            <p className="mt-2 text-sm text-slate-600">
                                {item.description}
                            </p>
                            <div className="mt-4 flex items-center justify-between">
                                <span className="text-xs font-medium text-slate-500">
                                    {item.totalTopics} topics
                                </span>
                                <span className="text-xs font-medium text-slate-500">
                                    {item.totalStudentsEnrolled} students
                                </span>
                            </div>
                        </Link>
                    ),
                )}
            </div>
        </section>
    );
}
