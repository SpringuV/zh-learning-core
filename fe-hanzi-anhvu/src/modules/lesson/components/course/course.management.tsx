"use client";

import {
    useCallback,
    useDeferredValue,
    useEffect,
    useMemo,
    useState,
} from "react";
import { toast } from "sonner";
import { CreateCourseModal } from "@/modules/lesson/components/course/create-course-modal";
import { CourseManagementTable } from "@/modules/lesson/components/course/course-management-table";
import { CourseStatusConfirmDialog } from "@/modules/lesson/components/course/course-status-confirm-dialog";
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
import {
    useDeleteCourse,
    useGetListCourse,
    usePublishCourse,
    useReOrderCourse,
    useUnPublishCourse,
} from "@/modules/lesson/hooks/use.course.tanstack";
import {
    CourseListItem,
    CourseListQueryParams,
    CourseSortBy,
} from "@/modules/lesson/types/coure.type";

const initialCourseQueryParams: CourseListQueryParams = {
    title: "",
    orderByDescending: true,
    hskLevel: 0,
    sortBy: "CreatedAt",
    take: 50,
};

const initialDraftCourseQuery: Omit<CourseListQueryParams, "title"> = {
    orderByDescending: true,
    hskLevel: 0,
    sortBy: "CreatedAt",
    take: 50,
};

const pageContainerClass = "mx-auto w-full max-w-[1400px] px-5";
const EMPTY_COURSES: CourseListItem[] = [];
// <T,> để đảm bảo hàm moveItem có thể nhận bất kỳ kiểu dữ liệu nào trong mảng, giúp hàm này trở nên linh hoạt và tái sử dụng được cho nhiều trường hợp khác nhau, không chỉ riêng với CourseListItem.

const moveItem = <T,>(items: T[], fromIndex: number, toIndex: number): T[] => {
    if (fromIndex === toIndex) {
        return items;
    }

    const nextItems = [...items];
    const [moved] = nextItems.splice(fromIndex, 1);
    if (!moved) {
        return items;
    }
    // Nếu toIndex vượt ra ngoài phạm vi, đặt nó ở cuối mảng
    nextItems.splice(toIndex, 0, moved);
    return nextItems;
};

export default function CourseCmsPage() {
    const [activeTab, setActiveTab] = useState<"courses" | "stats">("courses");
    const [itemsPerPage, setItemsPerPage] = useState(50);
    const [titleInput, setTitleInput] = useState("");
    const [draftQuery, setDraftQuery] = useState<
        Omit<CourseListQueryParams, "title">
    >(initialDraftCourseQuery);
    const [queryParams, setQueryParams] = useState<CourseListQueryParams>(
        initialCourseQueryParams,
    );
    const [currentPage, setCurrentPage] = useState(1);
    // isFilterDraftDirty sẽ so sánh các trường filter và sort trong draftQuery với queryParams để xác định xem có sự khác biệt nào không. Nếu có bất kỳ trường nào khác nhau, nó sẽ trả về true, cho biết rằng bộ lọc đã bị thay đổi và cần được áp dụng lại.
    const isFilterDraftDirty = useMemo(
        () =>
            draftQuery.hskLevel !== queryParams.hskLevel ||
            draftQuery.isPublished !== queryParams.isPublished ||
            draftQuery.sortBy !== queryParams.sortBy ||
            draftQuery.orderByDescending !== queryParams.orderByDescending,
        [draftQuery, queryParams],
    );

    const applyFilters = useCallback(() => {
        setCurrentPage(1);
        setQueryParams((current) => ({
            ...current,
            title: titleInput.trim(),
            hskLevel:
                draftQuery.hskLevel === 0 ? undefined : draftQuery.hskLevel,
            isPublished: draftQuery.isPublished,
            sortBy: draftQuery.sortBy,
            orderByDescending: draftQuery.orderByDescending,
        }));
    }, [draftQuery, titleInput]);
    const [pendingCourseId, setPendingCourseId] = useState<string | null>(null);
    const [isReorderMode, setIsReorderMode] = useState(false);
    const [orderedCourses, setOrderedCourses] = useState<CourseListItem[]>([]);
    const [isOrderDirty, setIsOrderDirty] = useState(false);
    const [publishDialogState, setPublishDialogState] = useState<{
        open: boolean;
        courseId: string | null;
        courseTitle: string;
        nextPublished: boolean;
    }>({
        open: false,
        courseId: null,
        courseTitle: "",
        nextPublished: false,
    });
    const [deleteDialogState, setDeleteDialogState] = useState<{
        open: boolean;
        courseId: string | null;
        courseTitle: string;
    }>({
        open: false,
        courseId: null,
        courseTitle: "",
    });
    // dùng debounce thay cho deferred value để tránh việc search khi người dùng đang gõ, chỉ search khi người dùng ngừng gõ một khoảng thời gian nhất định, giúp giảm số lượng request không cần thiết và cải thiện hiệu suất.
    useEffect(() => {
        const handler = setTimeout(() => {
            setQueryParams((current) => ({
                ...current,
                title: titleInput.trim(),
            }));
        }, 500);

        return () => clearTimeout(handler);
    }, [titleInput]);

    const effectiveQueryParams = useMemo<CourseListQueryParams>(
        () => ({
            ...queryParams,
            isPublished:
                queryParams.isPublished === undefined
                    ? undefined
                    : queryParams.isPublished,
            hskLevel:
                Number(queryParams.hskLevel) === 0
                    ? undefined
                    : Number(queryParams.hskLevel),
            title: queryParams.title?.trim() || "",
            take: itemsPerPage,
            page: currentPage,
        }),
        [queryParams, itemsPerPage, currentPage],
    );

    const coursesQuery = useGetListCourse(effectiveQueryParams);
    const publishCourseMutation = usePublishCourse();
    const unPublishCourseMutation = useUnPublishCourse();
    const reorderCourseMutation = useReOrderCourse();
    const deleteCourseMutation = useDeleteCourse();

    const currentPageData = coursesQuery.data?.data;
    const pageCourses = useMemo(
        () => currentPageData?.items ?? EMPTY_COURSES,
        [currentPageData?.items],
    );
    const loadedCourses = useMemo(() => pageCourses, [pageCourses]);

    const displayCourses = isReorderMode ? orderedCourses : pageCourses;

    const totalDocs = currentPageData?.pagination?.total ?? 0;
    const totalCourses = totalDocs;
    const publishedCourses = loadedCourses.filter(
        (course) => course.isPublished,
    ).length;
    const draftCourses = totalCourses - publishedCourses;
    const totalTopics = loadedCourses.reduce(
        (sum, course) => sum + course.totalTopics,
        0,
    );
    const totalStudents = loadedCourses.reduce(
        (sum, course) => sum + course.totalStudentsEnrolled,
        0,
    );

    const totalPages = Math.max(1, Math.ceil(totalDocs / itemsPerPage));
    const totalPagesForClamp =
        coursesQuery.isFetching && !currentPageData?.pagination
            ? Math.max(1, currentPage)
            : totalPages;
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = Math.min(startIndex + displayCourses.length, totalDocs);
    const totalItems = totalDocs;
    // #region ReOrder
    const reorderBaseCourses = pageCourses;
    const canPersistOrder =
        isReorderMode &&
        pageCourses.length > 0 &&
        orderedCourses.length === pageCourses.length;

    useEffect(() => {
        if (!isReorderMode) {
            return;
        }

        setOrderedCourses((current) => {
            if (
                current.length === reorderBaseCourses.length &&
                current.every(
                    (course, index) =>
                        course.id === reorderBaseCourses[index]?.id,
                )
            ) {
                return current;
            }

            return reorderBaseCourses;
        });
        setIsOrderDirty(false);
    }, [isReorderMode, reorderBaseCourses]);
    // #endregion
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
        queryParams.isPublished,
        queryParams.hskLevel,
        itemsPerPage,
    ]);

    const resetFilters = () => {
        setTitleInput("");
        setQueryParams(initialCourseQueryParams);
        setDraftQuery(initialDraftCourseQuery);
        setCurrentPage(1);
    };
    // #region reorder
    const startReorderMode = () => {
        setActiveTab("courses");
        setCurrentPage(1);
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
        setOrderedCourses([]);
        setIsOrderDirty(false);
    };

    const handleMoveCourse = useCallback((activeId: string, overId: string) => {
        setOrderedCourses((current) => {
            const sourceIndex = current.findIndex(
                (course) => course.id === activeId,
            );
            const targetIndex = current.findIndex(
                (course) => course.id === overId,
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

    const saveCourseOrder = async () => {
        if (!canPersistOrder || orderedCourses.length === 0) {
            toast.error(
                "Chưa đủ dữ liệu để lưu thứ tự. Vui lòng đợi tải xong dữ liệu trang hiện tại.",
            );
            return;
        }

        try {
            await reorderCourseMutation.mutateAsync({
                orderedCourseIds: orderedCourses.map((course) => course.id),
            });
            toast.success("Đã lưu thứ tự khóa học.");
            cancelReorderMode();
        } catch (error) {
            toast.error(getErrorMessage(error));
        }
    };
    // #endregion
    const getErrorMessage = (error: unknown) => {
        if (typeof error === "object" && error !== null) {
            const maybeApiError = error as {
                response?: {
                    data?: {
                        message?: string;
                        title?: string;
                    };
                };
                message?: string;
            };

            return (
                maybeApiError.response?.data?.message ??
                maybeApiError.response?.data?.title ??
                maybeApiError.message ??
                "Có lỗi xảy ra. Vui lòng thử lại."
            );
        }

        return "Có lỗi xảy ra. Vui lòng thử lại.";
    };

    const handleOpenPublishDialog = (
        courseId: string,
        courseTitle: string,
        nextPublished: boolean,
    ) => {
        setPublishDialogState({
            open: true,
            courseId,
            courseTitle,
            nextPublished,
        });
    };

    const handleConfirmPublishChange = async () => {
        if (!publishDialogState.courseId) {
            return;
        }

        const { courseId, nextPublished } = publishDialogState;

        setPendingCourseId(courseId);
        try {
            if (nextPublished) {
                await publishCourseMutation.mutateAsync(courseId);
                toast.success("Đã xuất bản khóa học.");
            } else {
                await unPublishCourseMutation.mutateAsync(courseId);
                toast.success("Đã hủy xuất bản khóa học.");
            }

            setPublishDialogState({
                open: false,
                courseId: null,
                courseTitle: "",
                nextPublished: false,
            });
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setPendingCourseId(null);
        }
    };

    const handleDeleteCourse = (courseId: string, title: string) => {
        setDeleteDialogState({
            open: true,
            courseId,
            courseTitle: title,
        });
    };

    const handleConfirmDeleteCourse = async () => {
        if (!deleteDialogState.courseId) {
            return;
        }

        setPendingCourseId(deleteDialogState.courseId);
        try {
            await deleteCourseMutation.mutateAsync(deleteDialogState.courseId);
            toast.success("Đã xóa khóa học.");
            setDeleteDialogState({
                open: false,
                courseId: null,
                courseTitle: "",
            });
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setPendingCourseId(null);
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

    return (
        <div className="bg-linear-to-br from-slate-50 via-white to-slate-50">
            <div className="sticky top-0 z-40 border-b border-slate-200/50 bg-white/80 backdrop-blur-lg">
                <div className={`${pageContainerClass} py-4`}>
                    <div className="flex items-center justify-between">
                        <div>
                            <h1 className="text-3xl font-light tracking-tight">
                                <span className="text-slate-900">Quản lý</span>
                                <span className="text-amber-600">
                                    {" "}
                                    khóa học
                                </span>
                            </h1>
                            <p className="mt-0.5 text-sm text-slate-500">
                                Tạo và quản lý khóa học, chủ đề và bài tập
                            </p>
                        </div>
                        <CreateCourseModal />
                    </div>

                    <div className="mt-3 rounded-xl border border-slate-200/70 bg-white p-3 shadow-sm">
                        <div className="flex flex-wrap items-center justify-between gap-3">
                            <div className="flex gap-1 rounded-lg  p-1">
                                {[
                                    {
                                        id: "courses",
                                        label: "Khóa học",
                                        count: totalCourses,
                                    },
                                    {
                                        id: "stats",
                                        label: "Thống kê",
                                        count: null,
                                    },
                                ].map((tab) => (
                                    <div
                                        key={tab.id}
                                        className="group relative"
                                    >
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
                                                    ? "text-orange-400! bg-white"
                                                    : "text-slate-600"
                                            }`}
                                        >
                                            {tab.label}
                                            {tab.count !== null && (
                                                <span className="ml-2 rounded bg-gray-100 px-2 py-1 text-xs">
                                                    {tab.count}
                                                </span>
                                            )}
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
                                Hiển thị {displayCourses.length}/{totalDocs}{" "}
                                khóa học
                            </p>
                        </div>

                        <div className="mt-3 flex flex-wrap gap-3">
                            <Input
                                type="text"
                                value={titleInput}
                                onChange={(event) =>
                                    setTitleInput(event.target.value)
                                }
                                placeholder="Tìm kiếm khóa học..."
                                className="h-9 min-w-64 flex-1 bg-white text-sm"
                                disabled={isReorderMode}
                            />

                            <Select
                                value={draftQuery.hskLevel?.toString() || "0"}
                                onValueChange={(value) =>
                                    setDraftQuery({
                                        ...draftQuery,
                                        hskLevel:
                                            value === "0"
                                                ? undefined
                                                : Number(value),
                                    })
                                }
                                disabled={isReorderMode}
                            >
                                <SelectTrigger className="h-9 w-32 bg-white text-sm">
                                    <SelectValue placeholder="HSK" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="0">Tất cả</SelectItem>
                                    <SelectItem value="1">HSK 1</SelectItem>
                                    <SelectItem value="2">HSK 2</SelectItem>
                                    <SelectItem value="3">HSK 3</SelectItem>
                                    <SelectItem value="4">HSK 4</SelectItem>
                                    <SelectItem value="5">HSK 5</SelectItem>
                                    <SelectItem value="6">HSK 6</SelectItem>
                                </SelectContent>
                            </Select>

                            <Select
                                value={
                                    draftQuery.isPublished === true
                                        ? "published"
                                        : draftQuery.isPublished === false
                                          ? "draft"
                                          : "all"
                                }
                                onValueChange={(value) =>
                                    setDraftQuery({
                                        ...draftQuery,
                                        isPublished:
                                            value === "published"
                                                ? true
                                                : value === "draft"
                                                  ? false
                                                  : undefined,
                                    })
                                }
                                disabled={isReorderMode}
                            >
                                <SelectTrigger className="h-9 w-44 bg-white text-sm">
                                    <SelectValue placeholder="Trạng thái" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">Tất cả</SelectItem>
                                    <SelectItem value="published">
                                        Đã xuất bản
                                    </SelectItem>
                                    <SelectItem value="draft">
                                        Bản nháp
                                    </SelectItem>
                                </SelectContent>
                            </Select>

                            <Select
                                value={draftQuery.sortBy ?? "CreatedAt"}
                                onValueChange={(value: CourseSortBy) =>
                                    setDraftQuery((current) => ({
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
                                    <SelectItem value="Title">
                                        Tiêu đề
                                    </SelectItem>
                                    <SelectItem value="HskLevel">
                                        Cấp độ HSK
                                    </SelectItem>
                                    <SelectItem value="TotalTopics">
                                        Tổng topic
                                    </SelectItem>
                                    <SelectItem value="TotalStudentsEnrolled">
                                        Tổng học viên
                                    </SelectItem>
                                </SelectContent>
                            </Select>

                            <Select
                                value={
                                    draftQuery.orderByDescending
                                        ? "desc"
                                        : "asc"
                                }
                                onValueChange={(value) =>
                                    setDraftQuery((current) => ({
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

                            <Button
                                type="button"
                                size="sm"
                                className="h-9 shrink-0"
                                onClick={applyFilters}
                                disabled={!isFilterDraftDirty || isReorderMode}
                            >
                                Áp dụng bộ lọc
                            </Button>

                            {!isReorderMode && (
                                <Button
                                    type="button"
                                    size="sm"
                                    className="h-9 shrink-0"
                                    onClick={startReorderMode}
                                    disabled={coursesQuery.isLoading}
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
                                        onClick={() => void saveCourseOrder()}
                                        disabled={
                                            reorderCourseMutation.isPending ||
                                            !isOrderDirty ||
                                            !canPersistOrder
                                        }
                                    >
                                        {reorderCourseMutation.isPending
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
                                            reorderCourseMutation.isPending
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
                    </div>
                </div>
            </div>

            <div className={`${pageContainerClass} min-w-0 pb-4 pt-3 lg:pb-5`}>
                {activeTab === "courses" && (
                    <div>
                        <div className="overflow-x-auto max-h-[50vh] rounded-xl border border-slate-200/50 bg-white shadow-sm">
                            <CourseManagementTable
                                courses={displayCourses}
                                isReorderMode={isReorderMode}
                                pendingCourseId={pendingCourseId}
                                isReorderPending={
                                    reorderCourseMutation.isPending
                                }
                                isLoading={coursesQuery.isLoading}
                                isError={coursesQuery.isError}
                                onMoveCourse={handleMoveCourse}
                                onOpenPublishDialog={handleOpenPublishDialog}
                                onDeleteCourse={handleDeleteCourse}
                            />
                        </div>

                        {!coursesQuery.isLoading &&
                            !coursesQuery.isError &&
                            pageCourses.length > 0 && (
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

                {activeTab === "stats" && (
                    <div className="space-y-4">
                        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
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
                                    className="rounded-xl border border-slate-200/50 bg-white p-5"
                                >
                                    <p className="mb-2 text-sm font-medium text-slate-600">
                                        {stat.label}
                                    </p>
                                    <p
                                        className={`mb-1 text-3xl font-light ${stat.tone}`}
                                    >
                                        {stat.value}
                                    </p>
                                </div>
                            ))}
                        </div>

                        <div className="rounded-xl border border-slate-200/50 bg-white p-5">
                            <p className="mb-2 text-sm font-medium text-slate-600">
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

            <CourseStatusConfirmDialog
                open={publishDialogState.open}
                onOpenChange={(open) =>
                    setPublishDialogState((current) => ({
                        ...current,
                        open,
                    }))
                }
                courseTitle={publishDialogState.courseTitle}
                nextPublished={publishDialogState.nextPublished}
                isSubmitting={Boolean(
                    publishDialogState.courseId &&
                    pendingCourseId === publishDialogState.courseId,
                )}
                onConfirm={handleConfirmPublishChange}
            />

            <ConfirmDialog
                open={deleteDialogState.open}
                onOpenChange={(open) => {
                    if (!open) {
                        setDeleteDialogState({
                            open: false,
                            courseId: null,
                            courseTitle: "",
                        });
                        return;
                    }

                    setDeleteDialogState((current) => ({
                        ...current,
                        open,
                    }));
                }}
                title="Xác nhận xóa khóa học"
                description={`Khóa học \"${deleteDialogState.courseTitle}\" sẽ bị xóa vĩnh viễn. Hành động này không thể hoàn tác.`}
                confirmLabel="Xóa khóa học"
                confirmClassName="bg-rose-600 text-white hover:bg-rose-700 focus-visible:ring-rose-500"
                isSubmitting={Boolean(
                    deleteDialogState.courseId &&
                    pendingCourseId === deleteDialogState.courseId,
                )}
                onConfirm={handleConfirmDeleteCourse}
            />
        </div>
    );
}
