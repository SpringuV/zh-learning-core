"use client";

import {
    useCallback,
    useDeferredValue,
    useEffect,
    useMemo,
    useState,
} from "react";
import Link from "next/link";
import { Eye, EyeOff, Trash2 } from "lucide-react";
import { useParams } from "next/navigation";
import { toast } from "sonner";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/components/ui/select";
import { CustomPagination } from "@/shared/components/cms/custom-pagination";
import {
    useDeleteExercise,
    useGetTopicExercisesOverview,
    usePublishExercise,
    useUnPublishExercise,
} from "@/modules/lesson/hooks/use.exercise.tanstack";
import {
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
    const [pendingExerciseId, setPendingExerciseId] = useState<string | null>(
        null,
    );

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
        }),
        [
            queryParams,
            deferredQuestion,
            skillFilter,
            exerciseTypeFilter,
            publishFilter,
            itemsPerPage,
        ],
    );

    const overviewQuery = useGetTopicExercisesOverview(
        normalizedTopicId,
        effectiveQueryParams,
    );

    const publishExerciseMutation = usePublishExercise();
    const unPublishExerciseMutation = useUnPublishExercise();
    const deleteExerciseMutation = useDeleteExercise();

    const overviewPages = overviewQuery.data?.pages ?? [];
    const loadedPages = overviewPages.length;
    const firstPageData = overviewPages[0]?.data;
    const currentPageData = overviewPages[currentPage - 1]?.data;
    const topicMetadata = firstPageData?.parentMetadata ?? null;
    const pageExercises = currentPageData?.items ?? [];

    const canLoadMore =
        loadedPages > 0
            ? (overviewPages[loadedPages - 1]?.data?.hasNextPage ?? false)
            : false;

    const totalDocs = firstPageData?.total ?? 0;
    const estimatedPages = loadedPages + (canLoadMore ? 1 : 0);
    const pagesFromTotal = Math.max(1, Math.ceil(totalDocs / itemsPerPage));
    const totalPages = Math.max(1, Math.min(estimatedPages, pagesFromTotal));
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = Math.min(startIndex + pageExercises.length, totalDocs);

    useEffect(() => {
        if (currentPage > totalPages) {
            setCurrentPage(totalPages);
        }
    }, [currentPage, totalPages]);

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
        const nextActionText = isPublished ? "hủy xuất bản" : "xuất bản";
        const confirmed = window.confirm(
            `Bạn có chắc muốn ${nextActionText} bài tập này không?`,
        );
        if (!confirmed) return;

        setPendingExerciseId(exerciseId);
        try {
            if (isPublished) {
                await unPublishExerciseMutation.mutateAsync(exerciseId);
                toast.success("Đã hủy xuất bản bài tập.");
            } else {
                await publishExerciseMutation.mutateAsync(exerciseId);
                toast.success("Đã xuất bản bài tập.");
            }
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setPendingExerciseId(null);
        }
    };

    const handleDeleteExercise = async (
        exerciseId: string,
        question: string,
    ) => {
        const confirmed = window.confirm(
            `Xóa bài tập \"${question}\"? Hành động này không thể hoàn tác.`,
        );
        if (!confirmed) return;

        setPendingExerciseId(exerciseId);
        try {
            await deleteExerciseMutation.mutateAsync(exerciseId);
            toast.success("Đã xóa bài tập.");
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setPendingExerciseId(null);
        }
    };

    const isExercisesLoading = overviewQuery.isLoading;

    return (
        <div className="min-h-full w-full min-w-0 bg-linear-to-br from-slate-50 via-white to-slate-50">
            <div className="border-b border-slate-200/50 bg-white/80 px-4 backdrop-blur-xl sm:px-6 lg:px-8">
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
                        <button className="rounded-md bg-slate-900 px-3 py-2 text-sm font-medium text-white transition-colors hover:bg-slate-800">
                            + Thêm bài tập
                        </button>
                    </div>
                </div>

                <div className="flex gap-4 overflow-x-auto pt-6">
                    {[
                        { id: "exercises", label: "Danh sách bài tập" },
                        { id: "settings", label: "Cài đặt topic" },
                    ].map((tab) => (
                        <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id as any)}
                            className={`relative shrink-0 pb-3 text-sm font-medium transition-colors duration-300 ease-out ${
                                activeTab === tab.id
                                    ? "text-amber-600"
                                    : "text-slate-500 hover:text-slate-800"
                            }`}
                        >
                            {tab.label}
                            <div
                                aria-hidden
                                className={`absolute bottom-0 left-0 z-10 h-0.5 w-full origin-center bg-amber-600 transition-transform duration-300 ease-out ${
                                    activeTab === tab.id
                                        ? "scale-x-100"
                                        : "scale-x-0"
                                }`}
                            />
                        </button>
                    ))}
                </div>
            </div>

            <div className="w-full min-w-0 px-4 py-6 sm:px-6 sm:py-8 lg:px-8">
                {activeTab === "exercises" && (
                    <div className="space-y-6">
                        <div className="flex flex-wrap items-center justify-end gap-3 rounded-xl border border-slate-200/50 bg-white p-4 shadow-sm">
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
                                className="h-9 w-72 bg-white text-sm"
                            />

                            <Select
                                value={skillFilter}
                                onValueChange={(value: SkillFilter) =>
                                    setSkillFilter(value)
                                }
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
                            >
                                <SelectTrigger className="h-9 w-36 bg-white text-sm">
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
                            >
                                <SelectTrigger className="h-9 w-36 bg-white text-sm">
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
                                className="h-9"
                                onClick={resetFilters}
                            >
                                Xóa lọc
                            </Button>
                        </div>

                        <div className="overflow-x-auto rounded-xl border border-slate-200/50 bg-white shadow-sm">
                            <table className="w-full min-w-full text-left text-sm">
                                <thead className="border-b border-slate-200/50 bg-slate-50/50">
                                    <tr>
                                        <th className="w-16 px-6 py-4 text-center font-semibold text-slate-600">
                                            STT
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Câu hỏi
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Loại
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Kỹ năng
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Độ khó
                                        </th>
                                        <th className="px-6 py-4 font-semibold text-slate-600">
                                            Trạng thái
                                        </th>
                                        <th className="px-6 py-4 text-right font-semibold text-slate-600">
                                            Thao tác
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {isExercisesLoading && (
                                        <tr>
                                            <td
                                                className="px-6 py-6 text-center text-slate-500"
                                                colSpan={7}
                                            >
                                                Đang tải bài tập...
                                            </td>
                                        </tr>
                                    )}

                                    {!isExercisesLoading &&
                                        overviewQuery.isError && (
                                            <tr>
                                                <td
                                                    className="px-6 py-6 text-center text-red-600"
                                                    colSpan={7}
                                                >
                                                    Không thể tải danh sách bài
                                                    tập.
                                                </td>
                                            </tr>
                                        )}

                                    {!isExercisesLoading &&
                                        !overviewQuery.isError &&
                                        pageExercises.length === 0 && (
                                            <tr>
                                                <td
                                                    className="px-6 py-6 text-center text-slate-500"
                                                    colSpan={7}
                                                >
                                                    Chưa có bài tập nào trong
                                                    topic này.
                                                </td>
                                            </tr>
                                        )}

                                    {!isExercisesLoading &&
                                        !overviewQuery.isError &&
                                        pageExercises.map((exercise, index) => (
                                            <tr
                                                key={exercise.exerciseId}
                                                className="group border-b border-slate-50 transition-colors hover:bg-slate-50/50"
                                            >
                                                <td className="px-6 py-4 text-center font-medium text-slate-400">
                                                    #{startIndex + index + 1}
                                                </td>
                                                <td className="px-6 py-4 font-medium text-slate-900">
                                                    {exercise.question}
                                                </td>
                                                <td className="px-6 py-4 text-slate-600">
                                                    {exerciseTypeLabels[
                                                        exercise.exerciseType
                                                    ] ?? exercise.exerciseType}
                                                </td>
                                                <td className="px-6 py-4 text-slate-600">
                                                    {skillLabels[
                                                        exercise.skillType
                                                    ] ?? exercise.skillType}
                                                </td>
                                                <td className="px-6 py-4 text-slate-600">
                                                    {exercise.difficulty}
                                                </td>
                                                <td className="px-6 py-4">
                                                    <Badge
                                                        variant="outline"
                                                        className={
                                                            exercise.isPublished
                                                                ? "border-emerald-200 bg-emerald-50 text-emerald-600"
                                                                : "border-slate-200 bg-slate-100 text-slate-600"
                                                        }
                                                    >
                                                        {exercise.isPublished
                                                            ? "Live"
                                                            : "Draft"}
                                                    </Badge>
                                                </td>
                                                <td className="px-6 py-4 text-right">
                                                    {pendingExerciseId ===
                                                        exercise.exerciseId && (
                                                        <p className="mb-2 text-[11px] text-slate-500">
                                                            Đang xử lý...
                                                        </p>
                                                    )}
                                                    <div className="flex justify-end gap-2">
                                                        <button
                                                            type="button"
                                                            disabled={
                                                                pendingExerciseId ===
                                                                exercise.exerciseId
                                                            }
                                                            onClick={() =>
                                                                void handleTogglePublishExercise(
                                                                    exercise.exerciseId,
                                                                    exercise.isPublished,
                                                                )
                                                            }
                                                            className="inline-flex items-center gap-1 rounded border border-slate-200 px-2 py-1 text-xs text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
                                                        >
                                                            {exercise.isPublished ? (
                                                                <>
                                                                    <EyeOff className="h-3.5 w-3.5" />
                                                                    Hủy xuất bản
                                                                </>
                                                            ) : (
                                                                <>
                                                                    <Eye className="h-3.5 w-3.5" />
                                                                    Xuất bản
                                                                </>
                                                            )}
                                                        </button>

                                                        <button
                                                            type="button"
                                                            disabled={
                                                                pendingExerciseId ===
                                                                exercise.exerciseId
                                                            }
                                                            onClick={() =>
                                                                void handleDeleteExercise(
                                                                    exercise.exerciseId,
                                                                    exercise.question,
                                                                )
                                                            }
                                                            className="inline-flex items-center gap-1 rounded border border-rose-200 px-2 py-1 text-xs text-rose-700 hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-60"
                                                        >
                                                            <Trash2 className="h-3.5 w-3.5" />
                                                            Xóa
                                                        </button>

                                                        <Link
                                                            href={`/cms/lessons/course/${normalizedCourseId}/topics/${normalizedTopicId}/exercises/${exercise.exerciseId}`}
                                                        >
                                                            <button className="rounded border border-slate-200 bg-white px-3 py-1.5 text-xs text-slate-600 shadow-sm transition-colors hover:bg-slate-50 hover:text-amber-600">
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
                        <h2 className="mb-3 text-xl font-semibold text-slate-900">
                            Cài đặt Topic
                        </h2>
                        <p className="text-sm text-slate-600">
                            Tính năng chỉnh sửa metadata topic ở màn này sẽ được
                            mở rộng ở bước tiếp theo.
                        </p>
                    </div>
                )}
            </div>
        </div>
    );
}
