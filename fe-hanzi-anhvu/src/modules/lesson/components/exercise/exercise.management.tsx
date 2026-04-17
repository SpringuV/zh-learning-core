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
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/components/ui/select";
import { CustomPagination } from "@/shared/components/cms/custom-pagination";
import { CreateExerciseModal } from "@/modules/lesson/components/exercise/create-exercise-modal";
import { ExerciseManagementTable } from "@/modules/lesson/components/exercise/exercise-management-table";
import {
    useDeleteExercise,
    useGetTopicExercisesOverview,
    usePublishExercise,
    useReOrderExercise,
    useUnPublishExercise,
} from "@/modules/lesson/hooks/use.exercise.tanstack";
import { useGetTopicDetail } from "@/modules/lesson/hooks/use.topic.tanstack";
import {
    ExerciseListItem,
    ExerciseListQueryParams,
    ExerciseSortBy,
    ExerciseType,
    SkillType,
} from "@/modules/lesson/types/exercise.type";

type ExerciseTypeFilter = ExerciseType | "all";
type SkillFilter = SkillType | "all";
type PublishFilter = "all" | "published" | "draft";

const initialExerciseQueryParams: ExerciseListQueryParams = {
    question: "",
    orderByDescending: true,
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

const exerciseTypeLabels: Record<ExerciseType, string> = {
    ListenDialogueChoice: "Nghe hội thoại",
    ListenImageChoice: "Nghe chọn hình",
    ListenFillBlank: "Nghe điền từ",
    ListenSentenceJudge: "Nghe đúng/sai",
    ReadFillBlank: "Đọc điền từ",
    ReadComprehension: "Đọc hiểu",
    ReadSentenceOrder: "Sắp xếp câu",
    ReadMatch: "Đọc nối",
    WriteHanzi: "Viết Hán tự",
    WritePinyin: "Viết Pinyin",
    WriteSentence: "Viết câu",
};

const skillLabels: Record<SkillType, string> = {
    Listening: "Nghe",
    Reading: "Đọc",
    Writing: "Viết",
    Speaking: "Nói",
};

export default function ExerciseManagementByTopic() {
    const params = useParams();
    const courseId = params["course-id"];
    const topicId = params["topic-id"];

    const normalizedCourseId = useMemo(
        () =>
            Array.isArray(courseId)
                ? (courseId[0] ?? "")
                : String(courseId ?? ""),
        [courseId],
    );

    const normalizedTopicId = useMemo(
        () =>
            Array.isArray(topicId) ? (topicId[0] ?? "") : String(topicId ?? ""),
        [topicId],
    );

    const [activeTab, setActiveTab] = useState<"exercises" | "settings">(
        "exercises",
    );
    const [itemsPerPage, setItemsPerPage] = useState(50);
    const [currentPage, setCurrentPage] = useState(1);
    const [queryParams, setQueryParams] = useState<ExerciseListQueryParams>(
        initialExerciseQueryParams,
    );
    const [skillFilter, setSkillFilter] = useState<SkillFilter>("all");
    const [exerciseTypeFilter, setExerciseTypeFilter] =
        useState<ExerciseTypeFilter>("all");
    const [publishFilter, setPublishFilter] = useState<PublishFilter>("all");
    const [isReorderMode, setIsReorderMode] = useState(false);
    const [orderedExercises, setOrderedExercises] = useState<
        ExerciseListItem[]
    >([]);
    const [isOrderDirty, setIsOrderDirty] = useState(false);
    const [pendingExerciseId, setPendingExerciseId] = useState<string | null>(
        null,
    );
    const [deleteDialogState, setDeleteDialogState] = useState<{
        open: boolean;
        exerciseId: string | null;
        exerciseQuestion: string;
    }>({
        open: false,
        exerciseId: null,
        exerciseQuestion: "",
    });
    const [publishDialogState, setPublishDialogState] = useState<{
        open: boolean;
        exerciseId: string | null;
        exerciseQuestion: string;
        nextPublished: boolean;
    }>({
        open: false,
        exerciseId: null,
        exerciseQuestion: "",
        nextPublished: false,
    });

    const deferredQuestion = useDeferredValue(queryParams.question ?? "");

    const effectiveQueryParams = useMemo<ExerciseListQueryParams>(
        () => ({
            ...queryParams,
            question: deferredQuestion.trim() || undefined,
            skillType: skillFilter === "all" ? undefined : skillFilter,
            exerciseType:
                exerciseTypeFilter === "all" ? undefined : exerciseTypeFilter,
            isPublished:
                publishFilter === "all"
                    ? undefined
                    : publishFilter === "published",
            take: itemsPerPage,
            page: currentPage,
        }),
        [
            queryParams,
            deferredQuestion,
            skillFilter,
            exerciseTypeFilter,
            publishFilter,
            itemsPerPage,
            currentPage,
        ],
    );

    const overviewQuery = useGetTopicExercisesOverview(
        normalizedTopicId,
        effectiveQueryParams,
    );
    const topicDetailQuery = useGetTopicDetail(normalizedTopicId);

    const publishExerciseMutation = usePublishExercise();
    const unPublishExerciseMutation = useUnPublishExercise();
    const reorderExerciseMutation = useReOrderExercise();
    const deleteExerciseMutation = useDeleteExercise();

    const currentPageData = overviewQuery.data?.data;
    const topicMetadata = currentPageData?.parentMetadata ?? null;
    const topicDetail = topicDetailQuery.data?.data;
    const effectiveTopicMetadata = topicDetail ?? topicMetadata;
    const pageExercises = currentPageData?.items ?? [];
    const displayExercises = isReorderMode ? orderedExercises : pageExercises;

    const totalDocs = currentPageData?.pagination?.total ?? 0;
    const totalPages = Math.max(1, Math.ceil(totalDocs / itemsPerPage));
    const totalPagesForClamp =
        overviewQuery.isFetching && !currentPageData?.pagination
            ? Math.max(1, currentPage)
            : totalPages;
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = Math.min(startIndex + displayExercises.length, totalDocs);
    const reorderBaseExercises = pageExercises;
    const canPersistOrder =
        isReorderMode &&
        pageExercises.length > 0 &&
        orderedExercises.length === pageExercises.length;

    useEffect(() => {
        if (!isReorderMode) {
            return;
        }

        setOrderedExercises((current) => {
            if (
                current.length === reorderBaseExercises.length &&
                current.every(
                    (exercise, index) =>
                        exercise.exerciseId ===
                        reorderBaseExercises[index]?.exerciseId,
                )
            ) {
                return current;
            }

            return reorderBaseExercises;
        });
        setIsOrderDirty(false);
    }, [isReorderMode, reorderBaseExercises]);

    useEffect(() => {
        if (currentPage > totalPagesForClamp) {
            setCurrentPage(totalPagesForClamp);
        }
    }, [currentPage, totalPagesForClamp]);

    useEffect(() => {
        setCurrentPage(1);
    }, [
        queryParams.question,
        queryParams.sortBy,
        queryParams.orderByDescending,
        skillFilter,
        exerciseTypeFilter,
        publishFilter,
        itemsPerPage,
    ]);

    const resetFilters = () => {
        setQueryParams(initialExerciseQueryParams);
        setSkillFilter("all");
        setExerciseTypeFilter("all");
        setPublishFilter("all");
        setCurrentPage(1);
    };

    const startReorderMode = () => {
        setActiveTab("exercises");
        setCurrentPage(1);
        setSkillFilter("all");
        setExerciseTypeFilter("all");
        setPublishFilter("all");
        setQueryParams((current) => ({
            ...current,
            question: "",
            sortBy: "OrderIndex",
            orderByDescending: false,
        }));
        setIsReorderMode(true);
    };

    const cancelReorderMode = () => {
        setIsReorderMode(false);
        setOrderedExercises([]);
        setIsOrderDirty(false);
    };

    const handleMoveExercise = useCallback(
        (activeId: string, overId: string) => {
            setOrderedExercises((current) => {
                const sourceIndex = current.findIndex(
                    (exercise) => exercise.exerciseId === activeId,
                );
                const targetIndex = current.findIndex(
                    (exercise) => exercise.exerciseId === overId,
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
        },
        [],
    );

    const saveExerciseOrder = async () => {
        if (!canPersistOrder || orderedExercises.length === 0) {
            toast.error(
                "Chưa đủ dữ liệu để lưu thứ tự. Vui lòng đợi tải xong dữ liệu trang hiện tại.",
            );
            return;
        }

        if (!normalizedTopicId) {
            toast.error("Không tìm thấy topicId để lưu thứ tự bài tập.");
            return;
        }

        try {
            await reorderExerciseMutation.mutateAsync({
                topicId: normalizedTopicId,
                orderedExerciseIds: orderedExercises.map(
                    (exercise) => exercise.exerciseId,
                ),
            });
            toast.success("Đã lưu thứ tự bài tập.");
            cancelReorderMode();
        } catch (error) {
            toast.error(getErrorMessage(error));
        }
    };

    const handlePageChange = useCallback(
        (page: number) => {
            if (page < 1 || page === currentPage) return;

            if (page > totalPages) return;
            setCurrentPage(page);
        },
        [currentPage, totalPages],
    );

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

    const handleTogglePublishExercise = async (
        exerciseId: string,
        isPublished: boolean,
    ) => {
        const targetExercise = displayExercises.find(
            (exercise) => exercise.exerciseId === exerciseId,
        );

        setPublishDialogState({
            open: true,
            exerciseId,
            exerciseQuestion: targetExercise?.question ?? "bài tập này",
            nextPublished: !isPublished,
        });
    };

    const handleConfirmPublishExercise = async () => {
        if (!publishDialogState.exerciseId) {
            return;
        }

        setPendingExerciseId(publishDialogState.exerciseId);
        try {
            if (publishDialogState.nextPublished) {
                await publishExerciseMutation.mutateAsync(
                    publishDialogState.exerciseId,
                );
                toast.success("Đã xuất bản bài tập.");
            } else {
                await unPublishExerciseMutation.mutateAsync(
                    publishDialogState.exerciseId,
                );
                toast.success("Đã hủy xuất bản bài tập.");
            }

            setPublishDialogState({
                open: false,
                exerciseId: null,
                exerciseQuestion: "",
                nextPublished: false,
            });
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setPendingExerciseId(null);
        }
    };

    const handleDeleteExercise = (exerciseId: string, question: string) => {
        setDeleteDialogState({
            open: true,
            exerciseId,
            exerciseQuestion: question,
        });
    };

    const handleConfirmDeleteExercise = async () => {
        if (!deleteDialogState.exerciseId) {
            return;
        }

        setPendingExerciseId(deleteDialogState.exerciseId);
        try {
            await deleteExerciseMutation.mutateAsync(
                deleteDialogState.exerciseId,
            );
            toast.success("Đã xóa bài tập.");
            setDeleteDialogState({
                open: false,
                exerciseId: null,
                exerciseQuestion: "",
            });
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setPendingExerciseId(null);
        }
    };

    const isExercisesLoading = overviewQuery.isLoading;

    const handleExerciseCreated = async () => {
        setCurrentPage(1);
    };

    return (
        <div className="min-h-full w-full min-w-0 bg-linear-to-br from-slate-50 via-white to-slate-50">
            <div className="sticky top-0 z-40 border-b border-slate-200/50 bg-white/80 px-4 backdrop-blur-xl sm:px-6 lg:px-8">
                <div className="flex min-w-0 flex-col gap-6 md:flex-row md:items-center md:justify-between">
                    <div className="min-w-0">
                        <div className="mb-2 flex items-center gap-3">
                            <Badge
                                variant="outline"
                                className="bg-slate-100 text-slate-700 border-slate-200"
                            >
                                Topic: {effectiveTopicMetadata?.title ?? "-"}
                            </Badge>
                            <Badge
                                variant="outline"
                                className="bg-amber-50 text-amber-700 border-amber-200"
                            >
                                Loại: {effectiveTopicMetadata?.topicType ?? "-"}
                            </Badge>
                            <Badge
                                variant="outline"
                                className="bg-blue-50 text-blue-700 border-blue-200"
                            >
                                Số bài:{" "}
                                {effectiveTopicMetadata?.totalExercises ?? 0}
                            </Badge>
                            <Badge
                                variant="secondary"
                                className={
                                    effectiveTopicMetadata?.isPublished
                                        ? "bg-emerald-100 text-emerald-700"
                                        : "bg-slate-100 text-slate-700"
                                }
                            >
                                {effectiveTopicMetadata?.isPublished
                                    ? "Đang xuất bản"
                                    : "Bản nháp"}
                            </Badge>
                        </div>
                        <h1 className="text-3xl font-semibold tracking-tight text-slate-900">
                            Quản lý bài tập theo Topic
                        </h1>
                        <p className="mt-2 max-w-2xl text-sm text-slate-500">
                            {effectiveTopicMetadata?.slug
                                ? `Slug: ${effectiveTopicMetadata.slug}`
                                : "Vui lòng chờ dữ liệu topic."}
                        </p>
                        {topicDetail?.description && (
                            <p className="mt-1 max-w-2xl text-sm text-slate-500">
                                {topicDetail.description}
                            </p>
                        )}
                        {overviewQuery.isError && (
                            <p className="mt-2 text-sm text-red-600">
                                Không thể tải danh sách bài tập.
                            </p>
                        )}
                        {topicDetailQuery.isError && (
                            <p className="mt-2 text-sm text-red-600">
                                Không thể tải chi tiết topic.
                            </p>
                        )}
                    </div>

                    <div className="flex w-full flex-wrap items-center justify-start gap-2 sm:gap-3 md:w-auto md:justify-end md:flex-nowrap">
                        <Link
                            href={`/cms/lessons/course/${normalizedCourseId}`}
                        >
                            <button className="rounded-md border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-50">
                                Về khóa học
                            </button>
                        </Link>
                        <CreateExerciseModal
                            topicId={normalizedTopicId}
                            onCreated={handleExerciseCreated}
                        />
                    </div>
                </div>

                <div className="mt-3 rounded-xl border border-slate-200/70 bg-white p-3 shadow-sm">
                    <div className="flex flex-wrap items-center justify-between gap-3">
                        <div className="flex gap-1 rounded-lg bg-slate-100/60 p-1">
                            {[
                                { id: "exercises", label: "Danh sách bài tập" },
                                { id: "settings", label: "Cài đặt topic" },
                            ].map((tab) => (
                                <Button
                                    type="button"
                                    key={tab.id}
                                    onClick={() => setActiveTab(tab.id as any)}
                                    variant={
                                        activeTab === tab.id
                                            ? "secondary"
                                            : "ghost"
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

                        <p className="text-xs text-slate-500 sm:text-sm">
                            Hiển thị {displayExercises.length}/{totalDocs} bài
                            tập
                        </p>
                    </div>
                </div>
            </div>

            <div className="w-full min-w-0 px-4 py-6 sm:px-6 sm:py-8 lg:px-8">
                {activeTab === "exercises" && (
                    <div className="space-y-6">
                        <div className="flex flex-wrap gap-3 rounded-xl border border-slate-200/50 bg-white p-4 shadow-sm">
                            <Input
                                type="text"
                                value={queryParams.question ?? ""}
                                onChange={(event) =>
                                    setQueryParams((current) => ({
                                        ...current,
                                        question: event.target.value,
                                    }))
                                }
                                placeholder="Tìm theo câu hỏi..."
                                className="h-9 min-w-64 flex-1 bg-white text-sm"
                                disabled={isReorderMode}
                            />

                            <Select
                                value={skillFilter}
                                onValueChange={(value: SkillFilter) =>
                                    setSkillFilter(value)
                                }
                                disabled={isReorderMode}
                            >
                                <SelectTrigger className="h-9 w-32 bg-white text-sm">
                                    <SelectValue placeholder="Kỹ năng" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">Tất cả</SelectItem>
                                    <SelectItem value="Listening">
                                        Nghe
                                    </SelectItem>
                                    <SelectItem value="Reading">Đọc</SelectItem>
                                    <SelectItem value="Writing">
                                        Viết
                                    </SelectItem>
                                    <SelectItem value="Speaking">
                                        Nói
                                    </SelectItem>
                                </SelectContent>
                            </Select>

                            <Select
                                value={exerciseTypeFilter}
                                onValueChange={(value: ExerciseTypeFilter) =>
                                    setExerciseTypeFilter(value)
                                }
                                disabled={isReorderMode}
                            >
                                <SelectTrigger className="h-9 w-44 bg-white text-sm">
                                    <SelectValue placeholder="Loại bài" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">
                                        Mọi loại
                                    </SelectItem>
                                    {Object.entries(exerciseTypeLabels).map(
                                        ([value, label]) => (
                                            <SelectItem
                                                key={value}
                                                value={value}
                                            >
                                                {label}
                                            </SelectItem>
                                        ),
                                    )}
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
                                onValueChange={(value: ExerciseSortBy) =>
                                    setQueryParams((current) => ({
                                        ...current,
                                        sortBy: value,
                                    }))
                                }
                                disabled={isReorderMode}
                            >
                                <SelectTrigger className="h-9 w-40 bg-white text-sm">
                                    <SelectValue placeholder="Sắp xếp" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="CreatedAt">
                                        Ngày tạo
                                    </SelectItem>
                                    <SelectItem value="UpdatedAt">
                                        Ngày cập nhật
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
                                        onClick={() => void saveExerciseOrder()}
                                        disabled={
                                            reorderExerciseMutation.isPending ||
                                            !isOrderDirty ||
                                            !canPersistOrder
                                        }
                                    >
                                        {reorderExerciseMutation.isPending
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
                                            reorderExerciseMutation.isPending
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

                        <div className="overflow-x-auto rounded-xl border border-slate-200/50 bg-white shadow-sm">
                            <ExerciseManagementTable
                                exercises={displayExercises}
                                startIndex={startIndex}
                                isReorderMode={isReorderMode}
                                isReorderPending={
                                    reorderExerciseMutation.isPending
                                }
                                pendingExerciseId={pendingExerciseId}
                                isLoading={isExercisesLoading}
                                isError={overviewQuery.isError}
                                normalizedCourseId={normalizedCourseId}
                                normalizedTopicId={normalizedTopicId}
                                exerciseTypeLabels={exerciseTypeLabels}
                                skillLabels={skillLabels}
                                onMoveExercise={handleMoveExercise}
                                onTogglePublishExercise={
                                    handleTogglePublishExercise
                                }
                                onDeleteExercise={handleDeleteExercise}
                            />
                        </div>
                        {!isExercisesLoading &&
                            !overviewQuery.isError &&
                            totalDocs > 0 && (
                                <CustomPagination
                                    currentPage={currentPage}
                                    totalPages={totalPages}
                                    itemsPerPage={itemsPerPage}
                                    startIndex={startIndex}
                                    endIndex={endIndex}
                                    totalItems={totalDocs}
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
                        <h2 className="mb-6 text-xl font-semibold text-slate-900">
                            Cài đặt Topic
                        </h2>

                        {topicDetailQuery.isLoading &&
                            !effectiveTopicMetadata && (
                                <p className="text-sm text-slate-600">
                                    Đang tải thông tin topic...
                                </p>
                            )}

                        {!topicDetailQuery.isLoading &&
                            !effectiveTopicMetadata && (
                                <p className="text-sm text-red-600">
                                    Không thể hiển thị thông tin topic.
                                </p>
                            )}

                        {effectiveTopicMetadata && (
                            <div className="space-y-6">
                                <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                                    <div>
                                        <label className="mb-2 block text-sm font-medium text-slate-700">
                                            Tên topic
                                        </label>
                                        <Input
                                            value={effectiveTopicMetadata.title}
                                            readOnly
                                            className="h-10 bg-slate-50"
                                        />
                                    </div>

                                    <div>
                                        <label className="mb-2 block text-sm font-medium text-slate-700">
                                            Slug
                                        </label>
                                        <Input
                                            value={effectiveTopicMetadata.slug}
                                            readOnly
                                            className="h-10 bg-slate-50"
                                        />
                                    </div>
                                </div>

                                {topicDetail?.description && (
                                    <div>
                                        <label className="mb-2 block text-sm font-medium text-slate-700">
                                            Mô tả
                                        </label>
                                        <textarea
                                            rows={4}
                                            value={topicDetail.description}
                                            readOnly
                                            className="w-full resize-none rounded-lg border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-slate-900"
                                        />
                                    </div>
                                )}

                                <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                                    <div>
                                        <label className="mb-2 block text-sm font-medium text-slate-700">
                                            Loại topic
                                        </label>
                                        <Input
                                            value={
                                                effectiveTopicMetadata.topicType
                                            }
                                            readOnly
                                            className="h-10 bg-slate-50"
                                        />
                                    </div>

                                    <div>
                                        <label className="mb-2 block text-sm font-medium text-slate-700">
                                            Trạng thái
                                        </label>
                                        <Input
                                            value={
                                                effectiveTopicMetadata.isPublished
                                                    ? "Đang xuất bản"
                                                    : "Bản nháp"
                                            }
                                            readOnly
                                            className="h-10 bg-slate-50"
                                        />
                                    </div>

                                    <div>
                                        <label className="mb-2 block text-sm font-medium text-slate-700">
                                            Thời lượng ước tính (phút)
                                        </label>
                                        <Input
                                            value={String(
                                                effectiveTopicMetadata.estimatedTimeMinutes ??
                                                    0,
                                            )}
                                            readOnly
                                            className="h-10 bg-slate-50"
                                        />
                                    </div>

                                    <div>
                                        <label className="mb-2 block text-sm font-medium text-slate-700">
                                            Tổng số bài tập
                                        </label>
                                        <Input
                                            value={String(
                                                effectiveTopicMetadata.totalExercises,
                                            )}
                                            readOnly
                                            className="h-10 bg-slate-50"
                                        />
                                    </div>
                                </div>

                                {effectiveTopicMetadata.topicType ===
                                    "Exam" && (
                                    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                                        <div>
                                            <label className="mb-2 block text-sm font-medium text-slate-700">
                                                Năm thi
                                            </label>
                                            <Input
                                                value={String(
                                                    effectiveTopicMetadata.examYear ??
                                                        "-",
                                                )}
                                                readOnly
                                                className="h-10 bg-slate-50"
                                            />
                                        </div>

                                        <div>
                                            <label className="mb-2 block text-sm font-medium text-slate-700">
                                                Mã đề
                                            </label>
                                            <Input
                                                value={
                                                    effectiveTopicMetadata.examCode ??
                                                    "-"
                                                }
                                                readOnly
                                                className="h-10 bg-slate-50"
                                            />
                                        </div>
                                    </div>
                                )}
                            </div>
                        )}
                    </div>
                )}
            </div>

            <ConfirmDialog
                open={deleteDialogState.open}
                onOpenChange={(open) => {
                    if (!open) {
                        setDeleteDialogState({
                            open: false,
                            exerciseId: null,
                            exerciseQuestion: "",
                        });
                        return;
                    }

                    setDeleteDialogState((current) => ({
                        ...current,
                        open,
                    }));
                }}
                title="Xác nhận xóa bài tập"
                description={`Bài tập \"${deleteDialogState.exerciseQuestion}\" sẽ bị xóa vĩnh viễn. Hành động này không thể hoàn tác.`}
                confirmLabel="Xóa bài tập"
                confirmClassName="bg-rose-600 text-white hover:bg-rose-700 focus-visible:ring-rose-500"
                isSubmitting={Boolean(
                    deleteDialogState.exerciseId &&
                    pendingExerciseId === deleteDialogState.exerciseId,
                )}
                onConfirm={handleConfirmDeleteExercise}
            />

            <ConfirmDialog
                open={publishDialogState.open}
                onOpenChange={(open) => {
                    if (!open) {
                        setPublishDialogState({
                            open: false,
                            exerciseId: null,
                            exerciseQuestion: "",
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
                        ? "Xác nhận xuất bản bài tập"
                        : "Xác nhận hủy xuất bản bài tập"
                }
                description={
                    publishDialogState.nextPublished
                        ? `Bài tập \"${publishDialogState.exerciseQuestion}\" sẽ được xuất bản.`
                        : `Bài tập \"${publishDialogState.exerciseQuestion}\" sẽ chuyển về trạng thái nháp.`
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
                    publishDialogState.exerciseId &&
                    pendingExerciseId === publishDialogState.exerciseId,
                )}
                onConfirm={handleConfirmPublishExercise}
            />
        </div>
    );
}
