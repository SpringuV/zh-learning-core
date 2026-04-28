"use client";

import {
    useCallback,
    useDeferredValue,
    useEffect,
    useMemo,
    useState,
} from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { toast } from "sonner";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog";
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
import { TopicManagementTable } from "@/modules/lesson/components/topic/topic-management-table";
import {
    useDeleteTopic,
    useGetCourseTopicsOverview,
    usePublishTopic,
    useReOrderTopic,
    useUnPublishTopic,
} from "@/modules/lesson/hooks/use.topic.tanstack";
import {
    TopicListItemAdmin,
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

const moveItem = <T,>(items: T[], fromIndex: number, toIndex: number): T[] => {
    if (fromIndex === toIndex) {
        return items;
    }

    const nextItems = [...items];
    const [moved] = nextItems.splice(fromIndex, 1);
    if (!moved) {
        return items;
    }

    nextItems.splice(toIndex, 0, moved);
    return nextItems;
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
    const [isReorderMode, setIsReorderMode] = useState(false);
    const [orderedTopics, setOrderedTopics] = useState<TopicListItemAdmin[]>(
        [],
    );
    const [isOrderDirty, setIsOrderDirty] = useState(false);
    const [deleteDialogState, setDeleteDialogState] = useState<{
        open: boolean;
        topicId: string | null;
        topicTitle: string;
    }>({
        open: false,
        topicId: null,
        topicTitle: "",
    });
    const [publishDialogState, setPublishDialogState] = useState<{
        open: boolean;
        topicId: string | null;
        topicTitle: string;
        nextPublished: boolean;
    }>({
        open: false,
        topicId: null,
        topicTitle: "",
        nextPublished: false,
    });

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
            page: currentPage,
        }),
        [
            queryParams,
            deferredTitle,
            topicTypeFilter,
            publishFilter,
            itemsPerPage,
            currentPage,
        ],
    );

    const overviewQuery = useGetCourseTopicsOverview(
        normalizedCourseId,
        effectiveQueryParams,
    );
    const publishTopicMutation = usePublishTopic();
    const unPublishTopicMutation = useUnPublishTopic();
    const reorderTopicMutation = useReOrderTopic();
    const deleteTopicMutation = useDeleteTopic();

    const currentPageData = overviewQuery.data?.data;
    const currentCourse = currentPageData?.parentMetadata ?? null;
    const pageTopics = currentPageData?.items ?? [];
    const displayTopics = isReorderMode ? orderedTopics : pageTopics;

    const totalDocs = currentPageData?.pagination?.total ?? 0;
    const totalPages = Math.max(1, Math.ceil(totalDocs / itemsPerPage));
    const totalPagesForClamp =
        overviewQuery.isFetching && !currentPageData?.pagination
            ? Math.max(1, currentPage)
            : totalPages;
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = Math.min(startIndex + displayTopics.length, totalDocs);
    const totalItems = totalDocs;
    const reorderBaseTopics = pageTopics;
    const canPersistOrder =
        isReorderMode &&
        pageTopics.length > 0 &&
        orderedTopics.length === pageTopics.length;

    useEffect(() => {
        if (!isReorderMode) {
            return;
        }

        setOrderedTopics((current) => {
            if (
                current.length === reorderBaseTopics.length &&
                current.every(
                    (topic, index) => topic.id === reorderBaseTopics[index]?.id,
                )
            ) {
                return current;
            }

            return reorderBaseTopics;
        });
        setIsOrderDirty(false);
    }, [isReorderMode, reorderBaseTopics]);

    const handlePageChange = useCallback(
        (page: number) => {
            if (page < 1 || page === currentPage) return;

            if (page > totalPages) return;
            setCurrentPage(page);
        },
        [currentPage, totalPages],
    );

    useEffect(() => {
        if (currentPage > totalPagesForClamp) {
            setCurrentPage(totalPagesForClamp);
        }
    }, [currentPage, totalPagesForClamp]);

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

    const startReorderMode = () => {
        setActiveTab("topics");
        setCurrentPage(1);
        setTopicTypeFilter("all");
        setPublishFilter("all");
        setQueryParams((current) => ({
            ...current,
            title: "",
            sortBy: "OrderIndex",
            orderByDescending: false,
        }));
        setIsReorderMode(true);
    };

    const cancelReorderMode = () => {
        setIsReorderMode(false);
        setOrderedTopics([]);
        setIsOrderDirty(false);
    };

    const handleMoveTopic = useCallback((activeId: string, overId: string) => {
        setOrderedTopics((current) => {
            const sourceIndex = current.findIndex(
                (topic) => topic.id === activeId,
            );
            const targetIndex = current.findIndex(
                (topic) => topic.id === overId,
            );

            if (sourceIndex < 0 || targetIndex < 0) {
                return current;
            }

            const next = moveItem(current, sourceIndex, targetIndex);
            if (next !== current) {
                setIsOrderDirty(true);
            }

            return next;
        });
    }, []);

    const saveTopicOrder = async () => {
        if (!canPersistOrder || orderedTopics.length === 0) {
            toast.error(
                "Chưa đủ dữ liệu để lưu thứ tự. Vui lòng đợi tải xong dữ liệu trang hiện tại.",
            );
            return;
        }

        if (!normalizedCourseId) {
            toast.error("Không tìm thấy courseId để lưu thứ tự topic.");
            return;
        }

        try {
            await reorderTopicMutation.mutateAsync({
                courseId: normalizedCourseId,
                orderedTopicIds: orderedTopics.map((topic) => topic.id),
            });
            toast.success("Đã lưu thứ tự topic.");
            cancelReorderMode();
        } catch (error) {
            toast.error(getErrorMessage(error));
        }
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
        const targetTopic = displayTopics.find((topic) => topic.id === topicId);

        setPublishDialogState({
            open: true,
            topicId,
            topicTitle: targetTopic?.title ?? "topic này",
            nextPublished: !isPublished,
        });
    };

    const handleConfirmPublishTopic = async () => {
        if (!publishDialogState.topicId) {
            return;
        }

        setPendingTopicId(publishDialogState.topicId);
        try {
            if (publishDialogState.nextPublished) {
                await publishTopicMutation.mutateAsync(
                    publishDialogState.topicId,
                );
                toast.success("Đã xuất bản topic.");
            } else {
                await unPublishTopicMutation.mutateAsync(
                    publishDialogState.topicId,
                );
                toast.success("Đã hủy xuất bản topic.");
            }

            setPublishDialogState({
                open: false,
                topicId: null,
                topicTitle: "",
                nextPublished: false,
            });
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setPendingTopicId(null);
        }
    };

    const handleDeleteTopic = (topicId: string, title: string) => {
        setDeleteDialogState({
            open: true,
            topicId,
            topicTitle: title,
        });
    };

    const handleConfirmDeleteTopic = async () => {
        if (!deleteDialogState.topicId) {
            return;
        }

        setPendingTopicId(deleteDialogState.topicId);
        try {
            await deleteTopicMutation.mutateAsync(deleteDialogState.topicId);
            toast.success("Đã xóa topic.");
            setDeleteDialogState({
                open: false,
                topicId: null,
                topicTitle: "",
            });
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

                <div className="flex flex-wrap items-center justify-between mt-2 p-3 gap-3">
                    <div className="flex gap-1 rounded-lg p-1">
                        {[
                            { id: "topics", label: "Quản lý Topics" },
                            { id: "settings", label: "Cài đặt khóa học" },
                        ].map((tab) => (
                            <div className="group relative" key={tab.id}>
                                <Button
                                    type="button"
                                    onClick={() => setActiveTab(tab.id as any)}
                                    variant={
                                        activeTab === tab.id
                                            ? "secondary"
                                            : "ghost"
                                    }
                                    size="sm"
                                    className={`rounded-md text-sm font-medium transition-all duration-300 ${
                                        activeTab === tab.id
                                            ? "text-orange-400! bg-white"
                                            : "text-slate-600"
                                    }`}
                                >
                                    {tab.label}
                                </Button>
                                <div
                                    aria-hidden
                                    className={`pointer-events-none absolute bottom-0 left-0 z-10 h-0.5 w-full origin-center bg-amber-600 transition-all duration-300 ease-out ${
                                        activeTab === tab.id
                                            ? "scale-x-100 opacity-100"
                                            : "scale-x-0 group-hover:scale-x-100"
                                    }`}
                                />
                            </div>
                        ))}
                    </div>

                    <div className="flex items-center gap-3">
                        <p className="text-xs text-slate-500 sm:text-sm">
                            Hiển thị {displayTopics.length}/{totalDocs} topics
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
                                disabled={isReorderMode}
                            />

                            <Select
                                value={topicTypeFilter}
                                onValueChange={(value: TopicTypeFilter) =>
                                    setTopicTypeFilter(value)
                                }
                                disabled={isReorderMode}
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
                                disabled={isReorderMode}
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
                                disabled={isReorderMode}
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
                                    <SelectItem value="OrderIndex">
                                        Thứ tự
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
                                disabled={isReorderMode}
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
                                disabled={isReorderMode}
                            >
                                Xóa lọc
                            </Button>

                            {!isReorderMode && (
                                <Button
                                    type="button"
                                    size="sm"
                                    className="h-9 shrink-0"
                                    onClick={startReorderMode}
                                    disabled={overviewQuery.isLoading}
                                >
                                    Sắp xếp kéo thả
                                </Button>
                            )}

                            {isReorderMode && (
                                <>
                                    <Button
                                        type="button"
                                        size="sm"
                                        className="h-9 shrink-0"
                                        onClick={() => void saveTopicOrder()}
                                        disabled={
                                            reorderTopicMutation.isPending ||
                                            !isOrderDirty ||
                                            !canPersistOrder
                                        }
                                    >
                                        {reorderTopicMutation.isPending
                                            ? "Đang lưu..."
                                            : "Lưu thứ tự"}
                                    </Button>
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="sm"
                                        className="h-9 shrink-0"
                                        onClick={cancelReorderMode}
                                        disabled={
                                            reorderTopicMutation.isPending
                                        }
                                    >
                                        Hủy
                                    </Button>
                                </>
                            )}

                            {isReorderMode && !canPersistOrder && (
                                <p className="w-full text-xs text-amber-700">
                                    Chỉ sắp xếp trong trang hiện tại. Vui lòng
                                    đợi tải xong dữ liệu trang trước khi lưu.
                                </p>
                            )}
                        </div>

                        <div className="overflow-x-auto max-h-[50vh] rounded-xl border border-slate-200/50 bg-white shadow-sm">
                            <TopicManagementTable
                                topics={displayTopics}
                                isReorderMode={isReorderMode}
                                isReorderPending={
                                    reorderTopicMutation.isPending
                                }
                                pendingTopicId={pendingTopicId}
                                isLoading={isTopicsLoading}
                                isError={overviewQuery.isError}
                                normalizedCourseId={normalizedCourseId}
                                onMoveTopic={handleMoveTopic}
                                onTogglePublishTopic={handleTogglePublishTopic}
                                onDeleteTopic={handleDeleteTopic}
                            />
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

            <ConfirmDialog
                open={deleteDialogState.open}
                onOpenChange={(open) => {
                    if (!open) {
                        setDeleteDialogState({
                            open: false,
                            topicId: null,
                            topicTitle: "",
                        });
                        return;
                    }

                    setDeleteDialogState((current) => ({
                        ...current,
                        open,
                    }));
                }}
                title="Xác nhận xóa topic"
                description={`Topic \"${deleteDialogState.topicTitle}\" sẽ bị xóa vĩnh viễn. Hành động này không thể hoàn tác.`}
                confirmLabel="Xóa topic"
                confirmClassName="bg-rose-600 text-white hover:bg-rose-700 focus-visible:ring-rose-500"
                isSubmitting={Boolean(
                    deleteDialogState.topicId &&
                    pendingTopicId === deleteDialogState.topicId,
                )}
                onConfirm={handleConfirmDeleteTopic}
            />

            <ConfirmDialog
                open={publishDialogState.open}
                onOpenChange={(open) => {
                    if (!open) {
                        setPublishDialogState({
                            open: false,
                            topicId: null,
                            topicTitle: "",
                            nextPublished: false,
                        });
                        return;
                    }

                    setPublishDialogState((current) => ({
                        ...current,
                        open,
                    }));
                }}
                title={
                    publishDialogState.nextPublished
                        ? "Xác nhận xuất bản topic"
                        : "Xác nhận hủy xuất bản topic"
                }
                description={
                    publishDialogState.nextPublished
                        ? `Topic \"${publishDialogState.topicTitle}\" sẽ được xuất bản.`
                        : `Topic \"${publishDialogState.topicTitle}\" sẽ chuyển về trạng thái nháp.`
                }
                confirmLabel={
                    publishDialogState.nextPublished
                        ? "Xuất bản"
                        : "Hủy xuất bản"
                }
                confirmClassName={
                    publishDialogState.nextPublished
                        ? undefined
                        : "bg-amber-600 text-white hover:bg-amber-700 focus-visible:ring-amber-500"
                }
                isSubmitting={Boolean(
                    publishDialogState.topicId &&
                    pendingTopicId === publishDialogState.topicId,
                )}
                onConfirm={handleConfirmPublishTopic}
            />
        </div>
    );
}
