"use client";
import { useGetCourseForDashboard } from "@/modules/lesson/hooks/use.course.tanstack";
import { CourseDashboardItem } from "@/modules/lesson/types/coure.type";
import { BookOpen, Play, Sparkles } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "@/shared/components/ui/button";

export function ClientDashboardMainContent() {
    const router = useRouter();
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

    const courses = courseDashboardList.data?.data ?? [];

    if (courses.length === 0) {
        return (
            <section className="min-h-0 flex-1 w-full overflow-y-auto px-4 py-6 md:px-6">
                <div className="rounded-[2rem] bg-linear-to-r from-emerald-600 via-emerald-500 to-slate-900 p-6 text-white shadow-2xl">
                    <p className="text-sm font-semibold uppercase tracking-[0.4em] text-emerald-100 opacity-80">
                        HanziAnhVu
                    </p>
                    <h1 className="mt-4 text-4xl font-semibold tracking-tight">
                        Chào mừng bạn đến với hành trình học tiếng Trung
                    </h1>
                    <p className="mt-4 max-w-2xl text-sm leading-6 text-white/80">
                        Hiện tại bạn chưa có khóa học nào. Hãy chọn một khóa học
                        phù hợp để bắt đầu học ngay hôm nay.
                    </p>
                    <div className="mt-6 flex flex-wrap gap-3">
                        <Button
                            variant="default"
                            size="sm"
                            onClick={() => router.back()}
                        >
                            Quay lại trang trước
                        </Button>
                        <Link
                            href="/u/dashboard"
                            className="inline-flex items-center justify-center gap-2 rounded-2xl border border-white/20 bg-white/10 px-4 py-2 text-sm font-semibold text-white transition hover:bg-white/20"
                        >
                            Khám phá khóa học
                        </Link>
                    </div>
                </div>
                <div className="mt-6 rounded-[2rem] border border-slate-200 bg-white p-8 shadow-sm">
                    <div className="flex flex-col items-center gap-4 text-center">
                        <Sparkles size={48} className="text-emerald-500" />
                        <h2 className="text-2xl font-semibold text-slate-900">
                            Bắt đầu học ngay
                        </h2>
                        <p className="max-w-lg text-sm text-slate-600">
                            Mỗi khóa học được thiết kế ngắn gọn, trực quan và dễ
                            tiếp cận. Chọn một khóa học để bắt đầu hành trình
                            của bạn.
                        </p>
                        <div className="grid w-full gap-3 sm:grid-cols-2">
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => router.back()}
                                className="w-full"
                            >
                                Quay lại
                            </Button>
                            <Link
                                href="/u/dashboard"
                                className="inline-flex h-full items-center justify-center rounded-2xl border border-slate-200 bg-slate-100 px-4 py-3 text-sm font-semibold text-slate-900 transition hover:bg-slate-200"
                            >
                                Khám phá khóa học
                            </Link>
                        </div>
                    </div>
                </div>
            </section>
        );
    }

    return (
        <section className="min-h-0 flex-1 w-full overflow-y-auto px-4 py-6 md:px-6">
            <div className="grid gap-6 lg:grid-cols-2 xl:grid-cols-3">
                {courses.map((item: CourseDashboardItem, index: number) => (
                    <article
                        key={item.id ?? index}
                        className="flex flex-col justify-between rounded-[1.75rem] border border-slate-200 bg-white p-6 shadow-md transition hover:-translate-y-1 hover:shadow-xl"
                    >
                        <div>
                            <div className="flex items-center justify-between gap-3">
                                <div>
                                    <p className="text-xs font-semibold uppercase tracking-[0.4em] text-emerald-700">
                                        Khóa học
                                    </p>
                                    <h2 className="mt-3 text-2xl font-semibold text-slate-900">
                                        {item.title}
                                    </h2>
                                </div>
                                <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
                                    HSK {item.hskLevel}
                                </span>
                            </div>
                            <p className="mt-5 text-sm leading-6 text-slate-600">
                                {item.description}
                            </p>
                            <div className="mt-6 grid gap-3 sm:grid-cols-2">
                                <div className="rounded-3xl bg-slate-50 p-4 text-sm text-slate-600">
                                    <p className="font-semibold text-slate-900">
                                        {item.totalTopics}
                                    </p>
                                    <p className="text-xs uppercase tracking-[0.2em] text-slate-500">
                                        Chủ đề
                                    </p>
                                </div>
                                <div className="rounded-3xl bg-slate-50 p-4 text-sm text-slate-600">
                                    <p className="font-semibold text-slate-900">
                                        {item.totalStudentsEnrolled}
                                    </p>
                                    <p className="text-xs uppercase tracking-[0.2em] text-slate-500">
                                        Học viên
                                    </p>
                                </div>
                            </div>
                        </div>
                        <div className="mt-6 flex flex-col gap-3">
                            <Link
                                href={`/u/${item.slug}`}
                                className="inline-flex items-center justify-center gap-2 rounded-2xl bg-emerald-700 px-4 py-3 text-sm font-semibold text-white transition hover:bg-emerald-600"
                            >
                                <Play className="size-4" />
                                Tiếp tục học
                            </Link>
                        </div>
                    </article>
                ))}
            </div>
        </section>
    );
}
