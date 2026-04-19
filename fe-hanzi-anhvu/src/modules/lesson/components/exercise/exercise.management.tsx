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
type ExerciseFilterState = {
    question: string;
    sortBy: ExerciseSortBy;
    orderByDescending: boolean;
    page: number;
    take: number;
    skill: SkillFilter;
    exerciseType: ExerciseTypeFilter;
    publishStatus: PublishFilter;
};

const initialFilterState: ExerciseFilterState = {
    question: "",
    orderByDescending: true,
    sortBy: "CreatedAt",
    page: 1,
    take: 50,
    skill: "all",
    exerciseType: "all",
    publishStatus: "all",
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

    // #region State
    const [activeTab, setActiveTab] = useState<"exercises" | "settings">(
        "exercises",
    );
    const [filterState, setFilterState] =
        useState<ExerciseFilterState>(initialFilterState);
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
    // #endregion

    const deferredQuestion = useDeferredValue(filterState.question);

    // Kết hợp các tham số truy vấn lại với nhau, ưu tiên giá trị đã được defer
    //  để tối ưu hiệu suất khi người dùng nhập liệu
    const effectiveQueryParams = useMemo<ExerciseListQueryParams>(
        () => ({
            question: deferredQuestion.trim() || undefined,
            orderByDescending: filterState.orderByDescending,
            sortBy: filterState.sortBy,
            skillType:
                filterState.skill === "all" ? undefined : filterState.skill,
            exerciseType:
                filterState.exerciseType === "all"
                    ? undefined
                    : filterState.exerciseType,
            isPublished:
                filterState.publishStatus === "all"
                    ? undefined
                    : filterState.publishStatus === "published",
            take: filterState.take,
            page: filterState.page,
        }),
        [
            deferredQuestion,
            filterState.orderByDescending,
            filterState.sortBy,
            filterState.skill,
            filterState.exerciseType,
            filterState.publishStatus,
            filterState.take,
            filterState.page,
        ],
    );
    // #region Data Fetching
    const overviewQuery = useGetTopicExercisesOverview(
        normalizedTopicId,
        effectiveQueryParams,
    );
    const publishExerciseMutation = usePublishExercise();
    const unPublishExerciseMutation = useUnPublishExercise();
    const reorderExerciseMutation = useReOrderExercise();
    const deleteExerciseMutation = useDeleteExercise();

    const currentPageData = overviewQuery.data?.data;
    const topicMetadata = currentPageData?.parentMetadata ?? null;
    const pageExercises = currentPageData?.items ?? [];
    const displayExercises = isReorderMode ? orderedExercises : pageExercises;

    const totalDocs = currentPageData?.pagination?.total ?? 0;
    const totalPages = Math.max(1, Math.ceil(totalDocs / filterState.take));
    const totalPagesForClamp =
        overviewQuery.isFetching && !currentPageData?.pagination
            ? Math.max(1, filterState.page)
            : totalPages;
    const startIndex = (filterState.page - 1) * filterState.take;
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
        if (filterState.page > totalPagesForClamp) {
            setFilterState((current) => ({
                ...current,
                page: totalPagesForClamp,
            }));
        }
    }, [filterState.page, totalPagesForClamp]);

    useEffect(() => {
        setFilterState((current) =>
            current.page === 1
                ? current
                : {
                      ...current,
                      page: 1,
                  },
        );
    }, [
        filterState.question,
        filterState.sortBy,
        filterState.orderByDescending,
        filterState.skill,
        filterState.exerciseType,
        filterState.publishStatus,
        filterState.take,
    ]);
    // #endregion

    const resetFilters = () => {
        setFilterState(initialFilterState);
    };

    const startReorderMode = () => {
        setActiveTab("exercises");
        setFilterState((current) => ({
            ...current,
            page: 1,
            skill: "all",
            exerciseType: "all",
            publishStatus: "all",
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
            if (page < 1 || page === filterState.page) return;

            if (page > totalPages) return;
            setFilterState((current) => ({
                ...current,
                page,
            }));
        },
        [filterState.page, totalPages],
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
        setFilterState((current) => ({
            ...current,
            page: 1,
        }));
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
                                Topic: {topicMetadata?.title ?? "-"}
                            </Badge>
                            <Badge
                                variant="outline"
                                className="bg-amber-50 text-amber-700 border-amber-200"
                            >
                                Loại: {topicMetadata?.topicType ?? "-"}
                            </Badge>
                            <Badge
                                variant="outline"
                                className="bg-blue-50 text-blue-700 border-blue-200"
                            >
                                Số bài: {topicMetadata?.totalExercises ?? 0}
                            </Badge>
                            <Badge
                                variant="secondary"
                                className={
                                    topicMetadata?.isPublished
                                        ? "bg-emerald-100 text-emerald-700"
                                        : "bg-slate-100 text-slate-700"
                                }
                            >
                                {topicMetadata?.isPublished
                                    ? "Đang xuất bản"
                                    : "Bản nháp"}
                            </Badge>
                        </div>
                        <h1 className="text-3xl font-semibold tracking-tight text-slate-900">
                            Quản lý bài tập theo Topic
                        </h1>
                        <p className="mt-2 max-w-2xl text-sm text-slate-500">
                            {topicMetadata?.slug
                                ? `Slug: ${topicMetadata.slug}`
                                : "Vui lòng chờ dữ liệu topic."}
                        </p>
                        {overviewQuery.isError && (
                            <p className="mt-2 text-sm text-red-600">
                                Không thể tải danh sách bài tập.
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
                        <div className="flex gap-1 rounded-lg p-1">
                            {[
                                { id: "exercises", label: "Danh sách bài tập" },
                                { id: "settings", label: "Cài đặt topic" },
                            ].map((tab) => (
                                <div key={tab.id} className="group relative">
                                    <Button
                                        type="button"
                                        onClick={() =>
                                            setActiveTab(tab.id as any)
                                        }
                                        variant={
                                            activeTab === tab.id
                                                ? "secondary"
                                                : "ghost"
                                        }
                                        size="sm"
                                        className={`rounded-md text-sm font-medium transition-all duration-300 ${
                                            activeTab === tab.id
                                                ? "bg-white text-slate-900 shadow-sm"
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
                                value={filterState.question}
                                onChange={(event) =>
                                    setFilterState((current) => ({
                                        ...current,
                                        question: event.target.value,
                                    }))
                                }
                                placeholder="Tìm theo câu hỏi..."
                                className="h-9 min-w-64 flex-1 bg-white text-sm"
                                disabled={isReorderMode}
                            />

                            <Select
                                value={filterState.skill}
                                onValueChange={(value: SkillFilter) =>
                                    setFilterState((current) => ({
                                        ...current,
                                        skill: value,
                                    }))
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
                                value={filterState.exerciseType}
                                onValueChange={(value: ExerciseTypeFilter) =>
                                    setFilterState((current) => ({
                                        ...current,
                                        exerciseType: value,
                                    }))
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
                                value={filterState.publishStatus}
                                onValueChange={(value: PublishFilter) =>
                                    setFilterState((current) => ({
                                        ...current,
                                        publishStatus: value,
                                    }))
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
                                value={filterState.sortBy}
                                onValueChange={(value: ExerciseSortBy) =>
                                    setFilterState((current) => ({
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
                                    filterState.orderByDescending
                                        ? "desc"
                                        : "asc"
                                }
                                onValueChange={(value) =>
                                    setFilterState((current) => ({
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
                                    currentPage={filterState.page}
                                    totalPages={totalPages}
                                    itemsPerPage={filterState.take}
                                    startIndex={startIndex}
                                    endIndex={endIndex}
                                    totalItems={totalDocs}
                                    totalDocs={totalDocs}
                                    onPageChange={handlePageChange}
                                    onItemsPerPageChange={(value) => {
                                        setFilterState((current) => ({
                                            ...current,
                                            take: value,
                                            page: 1,
                                        }));
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
                        {topicMetadata && (
                            <div className="space-y-6">
                                <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                                    <div>
                                        <label className="mb-2 block text-sm font-medium text-slate-700">
                                            Tên topic
                                        </label>
                                        <Input
                                            value={topicMetadata.title}
                                            readOnly
                                            className="h-10 bg-slate-50"
                                        />
                                    </div>

                                    <div>
                                        <label className="mb-2 block text-sm font-medium text-slate-700">
                                            Slug
                                        </label>
                                        <Input
                                            value={topicMetadata.slug}
                                            readOnly
                                            className="h-10 bg-slate-50"
                                        />
                                    </div>
                                </div>

                                <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                                    <div>
                                        <label className="mb-2 block text-sm font-medium text-slate-700">
                                            Loại topic
                                        </label>
                                        <Input
                                            value={topicMetadata.topicType}
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
                                                topicMetadata.isPublished
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
                                                topicMetadata.estimatedTimeMinutes ??
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
                                                topicMetadata.totalExercises,
                                            )}
                                            readOnly
                                            className="h-10 bg-slate-50"
                                        />
                                    </div>
                                </div>

                                {topicMetadata.topicType === "Exam" && (
                                    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                                        <div>
                                            <label className="mb-2 block text-sm font-medium text-slate-700">
                                                Năm thi
                                            </label>
                                            <Input
                                                value={String(
                                                    topicMetadata.examYear ??
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
                                                    topicMetadata.examCode ??
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
