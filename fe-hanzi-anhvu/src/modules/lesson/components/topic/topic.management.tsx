"use client";

import {
    useCallback,
    useDeferredValue,
    useEffect,
    useMemo,
    useState,
} from "react";
import Link from "next/link";
import { Trash2 } from "lucide-react";
import { useParams } from "next/navigation";
import { toast } from "sonner";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Switch } from "@/shared/components/ui/switch";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/components/ui/select";
import { CustomPagination } from "@/shared/components/cms/custom-pagination";
import { CreateTopicModal } from "@/modules/lesson/components/topic/create-topic-modal";
import {
    useDeleteTopic,
    useGetCourseTopicsOverview,
    usePublishTopic,
    useUnPublishTopic,
} from "@/modules/lesson/hooks/use.topic.tanstack";
import {
    TopicQueryParams,
    TopicSortBy,
    TopicType,
} from "@/modules/lesson/types/topic.type";

type TopicTypeFilter = TopicType | "all";
type PublishFilter = "all" | "published" | "draft";

const initialTopicQueryParams: TopicQueryParams = {
    title: "",
    orderByDescending: false,
    sortBy: "CreatedAt",
    take: 50,
};

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
    const [queryParams, setQueryParams] = useState<TopicQueryParams>(
        initialTopicQueryParams,
    );
    const [topicTypeFilter, setTopicTypeFilter] =
        useState<TopicTypeFilter>("all");
    const [publishFilter, setPublishFilter] = useState<PublishFilter>("all");
    const [pendingTopicId, setPendingTopicId] = useState<string | null>(null);

    const deferredTitle = useDeferredValue(queryParams.title ?? "");

    const effectiveQueryParams = useMemo<TopicQueryParams>(
        () => ({
            ...queryParams,
            title: deferredTitle.trim() || undefined,
            topicType: topicTypeFilter === "all" ? undefined : topicTypeFilter,
            isPublished:
                publishFilter === "all"
                    ? undefined
                    : publishFilter === "published",
            take: itemsPerPage,
        }),
        [
            queryParams,
            deferredTitle,
            topicTypeFilter,
            publishFilter,
            itemsPerPage,
        ],
    );

    const overviewQuery = useGetCourseTopicsOverview(
        normalizedCourseId,
        effectiveQueryParams,
    );
    const publishTopicMutation = usePublishTopic();
    const unPublishTopicMutation = useUnPublishTopic();
    const deleteTopicMutation = useDeleteTopic();

    const overviewPages = overviewQuery.data?.pages ?? [];
    const loadedPages = overviewPages.length;
    const firstPageData = overviewPages[0]?.data;
    const currentPageData = overviewPages[currentPage - 1]?.data;
    const currentCourse = overviewPages[0]?.data.parentMetadata ?? null;
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
    }, [
        queryParams.title,
        queryParams.sortBy,
        queryParams.orderByDescending,
        topicTypeFilter,
        publishFilter,
        itemsPerPage,
    ]);

    const resetFilters = () => {
        setQueryParams(initialTopicQueryParams);
        setTopicTypeFilter("all");
        setPublishFilter("all");
        setCurrentPage(1);
    };

    const getErrorMessage = (error: unknown) => {
        if (typeof error === "object" && error !== null) {
            const maybeApiError = error as {
                response?: {
                    data?: {
                        message?: string;
                        Message?: string;
                        title?: string;
                    };
                };
                message?: string;
            };

            return (
                maybeApiError.response?.data?.message ??
                maybeApiError.response?.data?.Message ??
                maybeApiError.response?.data?.title ??
                maybeApiError.message ??
                "Có lỗi xảy ra. Vui lòng thử lại."
            );
        }

        return "Có lỗi xảy ra. Vui lòng thử lại.";
    };

    const handleTogglePublishTopic = async (
        topicId: string,
        isPublished: boolean,
    ) => {
        const nextActionText = isPublished ? "hủy xuất bản" : "xuất bản";
        const confirmed = window.confirm(
            `Bạn có chắc muốn ${nextActionText} topic này không?`,
        );
        if (!confirmed) return;

        setPendingTopicId(topicId);
        try {
            if (isPublished) {
                await unPublishTopicMutation.mutateAsync(topicId);
                toast.success("Đã hủy xuất bản topic.");
            } else {
                await publishTopicMutation.mutateAsync(topicId);
                toast.success("Đã xuất bản topic.");
            }
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setPendingTopicId(null);
        }
    };

    const handleDeleteTopic = async (topicId: string, title: string) => {
        const confirmed = window.confirm(
            `Xóa topic \"${title}\"? Hành động này không thể hoàn tác.`,
        );
        if (!confirmed) return;

        setPendingTopicId(topicId);
        try {
            await deleteTopicMutation.mutateAsync(topicId);
            toast.success("Đã xóa topic.");
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setPendingTopicId(null);
        }
    };

    const onTopicCreated = async () => {
        setCurrentPage(1);
        // await overviewQuery.refetch();
    };

    const isTopicsLoading = overviewQuery.isLoading;

    return (
        <div className="min-h-full w-full min-w-0 bg-linear-to-br from-slate-50 via-white to-slate-50">
            {/* Header Section */}
            <div className="sticky top-0 z-40 border-b border-slate-200/50 bg-white/80 backdrop-blur-xl sm:px-3 lg:px-8">
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

                    <div className="flex w-full flex-wrap items-center justify-start gap-2 sm:gap-3 md:w-auto md:justify-end md:flex-nowrap">
                        {activeTab === "topics" && (
                            <CreateTopicModal
                                courseId={normalizedCourseId}
                                onCreated={onTopicCreated}
                            />
                        )}
                        <Link href="/cms/lessons/course">
                            <button className="px-3 py-2 bg-white border border-slate-200 text-slate-700 rounded-md hover:bg-slate-50 transition-colors font-medium text-sm">
                                Quay lại
                            </button>
                        </Link>
                    </div>
                </div>

                <div className="flex flex-wrap items-center justify-between mt-2 gap-3">
                    <div className="flex gap-1 rounded-lg bg-slate-100/60 p-1">
                        {[
                            { id: "topics", label: "Quản lý Topics" },
                            { id: "settings", label: "Cài đặt khóa học" },
                        ].map((tab) => (
                            <Button
                                type="button"
                                key={tab.id}
                                onClick={() => setActiveTab(tab.id as any)}
                                variant={
                                    activeTab === tab.id ? "secondary" : "ghost"
                                }
                                size="sm"
                                className={`rounded-md text-sm font-medium transition-all duration-300 ${
                                    activeTab === tab.id
                                        ? "bg-white text-slate-900 shadow-sm"
                                        : "text-slate-600 hover:text-slate-900"
                                }`}
                            >
                                {tab.label}
                            </Button>
                        ))}
                    </div>

                    <div className="flex items-center gap-3">
                        <p className="text-xs text-slate-500 sm:text-sm">
                            Hiển thị {pageTopics.length}/{totalDocs} topics
                        </p>
                    </div>
                </div>
            </div>

            {/* Tab Contents */}
            <div className="w-full min-w-0 px-4 py-6 sm:px-6 sm:py-4 lg:px-8">
                {activeTab === "topics" && (
                    <div className="space-y-6">
                        <div className="flex flex-wrap gap-3 rounded-xl border border-slate-200/50 bg-white p-4 shadow-sm">
                            <Input
                                type="text"
                                value={queryParams.title ?? ""}
                                onChange={(event) =>
                                    setQueryParams((current) => ({
                                        ...current,
                                        title: event.target.value,
                                    }))
                                }
                                placeholder="Tìm theo tên topic..."
                                className="h-9 min-w-64 flex-1 bg-white text-sm"
                            />

                            <Select
                                value={topicTypeFilter}
                                onValueChange={(value: TopicTypeFilter) =>
                                    setTopicTypeFilter(value)
                                }
                            >
                                <SelectTrigger className="h-9 w-36 bg-white text-sm">
                                    <SelectValue placeholder="Loại topic" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">
                                        Mọi loại
                                    </SelectItem>
                                    <SelectItem value="Learning">
                                        Learning
                                    </SelectItem>
                                    <SelectItem value="Exam">Exam</SelectItem>
                                </SelectContent>
                            </Select>

                            <Select
                                value={publishFilter}
                                onValueChange={(value: PublishFilter) =>
                                    setPublishFilter(value)
                                }
                            >
                                <SelectTrigger className="h-9 w-44 bg-white text-sm">
                                    <SelectValue placeholder="Trạng thái" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">
                                        Mọi trạng thái
                                    </SelectItem>
                                    <SelectItem value="published">
                                        Đã xuất bản
                                    </SelectItem>
                                    <SelectItem value="draft">
                                        Bản nháp
                                    </SelectItem>
                                </SelectContent>
                            </Select>

                            <Select
                                value={queryParams.sortBy ?? "CreatedAt"}
                                onValueChange={(value: TopicSortBy) =>
                                    setQueryParams((current) => ({
                                        ...current,
                                        sortBy: value,
                                    }))
                                }
                            >
                                <SelectTrigger className="h-9 w-40 bg-white text-sm">
                                    <SelectValue placeholder="Sắp xếp theo" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="CreatedAt">
                                        Ngày tạo
                                    </SelectItem>
                                    <SelectItem value="UpdatedAt">
                                        Ngày cập nhật
                                    </SelectItem>
                                    <SelectItem value="TotalExercises">
                                        Số bài tập
                                    </SelectItem>
                                    <SelectItem value="ExamYear">
                                        Năm thi
                                    </SelectItem>
                                </SelectContent>
                            </Select>

                            <Select
                                value={
                                    queryParams.orderByDescending
                                        ? "desc"
                                        : "asc"
                                }
                                onValueChange={(value) =>
                                    setQueryParams((current) => ({
                                        ...current,
                                        orderByDescending: value === "desc",
                                    }))
                                }
                            >
                                <SelectTrigger className="h-9 w-36 bg-white text-sm">
                                    <SelectValue placeholder="Thứ tự" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="desc">
                                        Giảm dần
                                    </SelectItem>
                                    <SelectItem value="asc">
                                        Tăng dần
                                    </SelectItem>
                                </SelectContent>
                            </Select>

                            <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                className="h-9 shrink-0"
                                onClick={resetFilters}
                            >
                                Xóa lọc
                            </Button>
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
                                                    <div className="flex items-center gap-2">
                                                        <Switch
                                                            checked={
                                                                topic.isPublished
                                                            }
                                                            size="default"
                                                            className="h-6 w-11 border border-slate-300 ring-1 ring-slate-200 data-checked:bg-emerald-600 data-unchecked:bg-slate-300"
                                                            disabled={
                                                                pendingTopicId ===
                                                                topic.id
                                                            }
                                                            onCheckedChange={() =>
                                                                void handleTogglePublishTopic(
                                                                    topic.id,
                                                                    topic.isPublished,
                                                                )
                                                            }
                                                            aria-label={`Chuyển trạng thái xuất bản của ${topic.title}`}
                                                        />
                                                        <Badge
                                                            className={
                                                                topic.isPublished
                                                                    ? "bg-green-100 text-green-700"
                                                                    : "bg-slate-100 text-slate-600"
                                                            }
                                                        >
                                                            {topic.isPublished
                                                                ? "Đã xuất bản"
                                                                : "Nháp"}
                                                        </Badge>
                                                    </div>
                                                </td>
                                                <td className="px-6 py-4 text-right">
                                                    {pendingTopicId ===
                                                        topic.id && (
                                                        <p className="mb-2 text-[11px] text-slate-500">
                                                            Đang xử lý...
                                                        </p>
                                                    )}
                                                    <div className="flex justify-end gap-2">
                                                        <button
                                                            type="button"
                                                            disabled={
                                                                pendingTopicId ===
                                                                topic.id
                                                            }
                                                            onClick={() =>
                                                                void handleDeleteTopic(
                                                                    topic.id,
                                                                    topic.title,
                                                                )
                                                            }
                                                            className="inline-flex items-center gap-1 rounded border border-rose-200 px-2 py-1 text-xs text-rose-700 hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-60"
                                                        >
                                                            <Trash2 className="h-3.5 w-3.5" />
                                                            Xóa
                                                        </button>

                                                        <Link
                                                            href={`/cms/lessons/course/${normalizedCourseId}/topics/${topic.id}`}
                                                        >
                                                            <button className="px-3 py-1.5 text-xs bg-white border border-slate-200 text-slate-600 rounded hover:bg-slate-50 hover:text-amber-600 transition-colors shadow-sm">
                                                                Quản lý bài tập
                                                            </button>
                                                        </Link>
                                                    </div>
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
