"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { Badge } from "@/shared/components/ui/badge";
import { CustomPagination } from "@/shared/components/cms/custom-pagination";
import { CreateTopicModal } from "@/modules/lesson/components/topic/create-topic-modal";
import { useGetCourseTopicsOverview } from "@/modules/lesson/hooks/use.topic.tanstack";

export default function TopicManagementByCourse() {
    const params = useParams();
    const courseId = params["course-id"];
    // courseId maybe string or string[], Depending on how routes are defined in Next.js,
    // they need to be normalized to strings before being used in queries later
    const normalizedCourseId = useMemo(
        () =>
            Array.isArray(courseId)
                ? (courseId[0] ?? "")
                : String(courseId ?? ""),
        [courseId],
    );

    const [activeTab, setActiveTab] = useState<"topics" | "settings">("topics");
    const [itemsPerPage, setItemsPerPage] = useState(50);
    const [currentPage, setCurrentPage] = useState(1);

    const overviewQuery = useGetCourseTopicsOverview(normalizedCourseId, {
        take: itemsPerPage,
        orderByDescending: false,
    });

    const overviewPages = overviewQuery.data?.pages ?? [];
    const loadedPages = overviewPages.length;
    const firstPageData = overviewPages[0]?.data;
    const currentPageData = overviewPages[currentPage - 1]?.data;
    const currentCourse = overviewPages[0]?.data.course ?? null;
    const pageTopics = currentPageData?.items ?? [];

    const canLoadMore =
        loadedPages > 0
            ? (overviewPages[loadedPages - 1]?.data?.hasNextPage ?? false)
            : false;

    const totalDocs = firstPageData?.total ?? 0;
    const estimatedPages = loadedPages + (canLoadMore ? 1 : 0);
    const pagesFromTotal = Math.max(1, Math.ceil(totalDocs / itemsPerPage));
    const totalPages = Math.max(1, Math.min(estimatedPages, pagesFromTotal));
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = Math.min(startIndex + pageTopics.length, totalDocs);
    const totalItems = totalDocs;

    const handlePageChange = useCallback(
        (page: number) => {
            if (page < 1 || page === currentPage) return;

            if (page <= loadedPages) {
                setCurrentPage(page);
                return;
            }

            if (
                page === loadedPages + 1 &&
                canLoadMore &&
                !overviewQuery.isFetchingNextPage
            ) {
                void overviewQuery.fetchNextPage().then((result) => {
                    const fetchedPageCount =
                        result.data?.pages.length ?? loadedPages;
                    if (fetchedPageCount >= page) {
                        setCurrentPage(page);
                    }
                });
            }
        },
        [currentPage, loadedPages, canLoadMore, overviewQuery],
    );

    useEffect(() => {
        if (currentPage > totalPages) {
            setCurrentPage(totalPages);
        }
    }, [currentPage, totalPages]);

    useEffect(() => {
        setCurrentPage(1);
    }, [itemsPerPage]);

    const onTopicCreated = async () => {
        setCurrentPage(1);
        await overviewQuery.refetch();
    };

    const isTopicsLoading = overviewQuery.isLoading;

    return (
        <div className="min-h-full w-full min-w-0 bg-linear-to-br from-slate-50 via-white to-slate-50">
            {/* Header Section */}
            <div className="border-b border-slate-200/50 bg-white/80 px-4 backdrop-blur-xl sm:px-6 lg:px-8">
                <div className="flex min-w-0 flex-col gap-6 md:flex-row md:items-center md:justify-between">
                    <div className="min-w-0">
                        <div className="flex items-center gap-3 mb-2">
                            <Badge
                                variant="outline"
                                className="bg-amber-50 text-amber-700 border-amber-200"
                            >
                                HSK {currentCourse?.hskLevel ?? "-"}
                            </Badge>
                            <Badge
                                variant="secondary"
                                className={
                                    currentCourse?.isPublished
                                        ? "bg-emerald-100 text-emerald-700"
                                        : "bg-slate-100 text-slate-700"
                                }
                            >
                                {currentCourse?.isPublished
                                    ? "Đang xuất bản"
                                    : "Bản nháp"}
                            </Badge>
                        </div>
                        <h1 className="text-3xl font-semibold text-slate-900 tracking-tight wrap-break-word">
                            {currentCourse?.title || "Đang tải khóa học..."}
                        </h1>
                        <p className="text-slate-500 mt-2 max-w-2xl text-sm">
                            {currentCourse?.slug
                                ? `Slug: ${currentCourse.slug}`
                                : "Vui lòng chờ dữ liệu khóa học."}
                        </p>
                        {overviewQuery.isError && (
                            <p className="mt-2 text-sm text-red-600">
                                Không thể tải thông tin khóa học.
                            </p>
                        )}
                    </div>

                    <div className="flex flex-1 w-full items-center justify-evenly flex-wrap gap-3 md:w-auto md:flex-nowrap">
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
                <div className="flex gap-8 overflow-x-auto pt-8">
                    {[
                        { id: "topics", label: "Quản lý Topics" },
                        { id: "settings", label: "Cài đặt khóa học" },
                    ].map((tab) => (
                        <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id as any)}
                            className={`relative shrink-0 pb-4 text-sm font-medium transition-colors ${
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
            <div className="w-full min-w-0 px-4 py-6 sm:px-6 sm:py-8 lg:px-8">
                {activeTab === "topics" && (
                    <div className="space-y-6">
                        <div className="flex flex-col gap-4 rounded-xl border border-slate-200/50 bg-white p-5 shadow-sm sm:flex-row sm:items-center sm:justify-between">
                            <div>
                                <h2 className="text-lg font-semibold text-slate-900">
                                    Nội dung bài học (Topics)
                                </h2>
                                <p className="text-sm text-slate-500">
                                    Quản lý các chương/chủ đề trong khóa học này
                                </p>
                            </div>
                            <CreateTopicModal
                                courseId={normalizedCourseId}
                                onCreated={onTopicCreated}
                            />
                        </div>

                        <div className="overflow-x-auto rounded-xl border border-slate-200/50 bg-white shadow-sm">
                            <table className="w-full min-w-full text-sm text-left">
                                <thead className="bg-slate-50/50 border-b border-slate-200/50">
                                    <tr>
                                        <th className="px-6 py-4 font-semibold text-slate-600 w-16 text-center">
                                            STT
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Tên Topic
                                        </th>
                                        <th className="hidden sm:table-cell px-6 py-4 font-semibold text-slate-600">
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
                                    {isTopicsLoading && (
                                        <tr>
                                            <td
                                                className="px-6 py-6 text-center text-slate-500"
                                                colSpan={5}
                                            >
                                                Đang tải topics...
                                            </td>
                                        </tr>
                                    )}

                                    {!isTopicsLoading &&
                                        overviewQuery.isError && (
                                            <tr>
                                                <td
                                                    className="px-6 py-6 text-center text-red-600"
                                                    colSpan={5}
                                                >
                                                    Không thể tải danh sách
                                                    topics.
                                                </td>
                                            </tr>
                                        )}

                                    {!isTopicsLoading &&
                                        !overviewQuery.isError &&
                                        pageTopics.length === 0 && (
                                            <tr>
                                                <td
                                                    className="px-6 py-6 text-center text-slate-500"
                                                    colSpan={5}
                                                >
                                                    Chưa có topic nào trong khóa
                                                    học này.
                                                </td>
                                            </tr>
                                        )}

                                    {!isTopicsLoading &&
                                        !overviewQuery.isError &&
                                        pageTopics.map((topic) => (
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
                                                <td className="hidden sm:table-cell px-6 py-4 text-slate-500">
                                                    {topic.totalExercises} bài
                                                    tập
                                                </td>
                                                <td className="px-6 py-4">
                                                    <Badge
                                                        variant="outline"
                                                        className={
                                                            topic.isPublished
                                                                ? "bg-emerald-50 text-emerald-600 border-emerald-200"
                                                                : "bg-slate-100 text-slate-600 border-slate-200"
                                                        }
                                                    >
                                                        {topic.isPublished
                                                            ? "Live"
                                                            : "Draft"}
                                                    </Badge>
                                                </td>
                                                <td className="px-6 py-4 text-right">
                                                    <Link
                                                        href={`/cms/lessons/course/${normalizedCourseId}/topics/${topic.id}`}
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

                        {!isTopicsLoading &&
                            !overviewQuery.isError &&
                            totalDocs > 0 && (
                                <CustomPagination
                                    currentPage={currentPage}
                                    totalPages={totalPages}
                                    itemsPerPage={itemsPerPage}
                                    startIndex={startIndex}
                                    endIndex={endIndex}
                                    totalItems={totalItems}
                                    totalDocs={totalDocs}
                                    onPageChange={handlePageChange}
                                    onItemsPerPageChange={(value) => {
                                        setItemsPerPage(value);
                                        setCurrentPage(1);
                                    }}
                                />
                            )}
                    </div>
                )}

                {activeTab === "settings" && (
                    <div className="w-full max-w-3xl rounded-xl border border-slate-200/50 bg-white p-4 shadow-sm sm:p-6 lg:p-8">
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
                                    defaultValue={currentCourse?.title ?? ""}
                                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all"
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-2">
                                    Mô tả (Description)
                                </label>
                                <textarea
                                    rows={4}
                                    defaultValue={currentCourse?.slug ?? ""}
                                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all resize-none"
                                ></textarea>
                            </div>

                            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-2">
                                        Cấp độ (HSK Level)
                                    </label>
                                    <select
                                        defaultValue={
                                            currentCourse?.hskLevel ?? 1
                                        }
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
                                                    currentCourse?.isPublished
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
