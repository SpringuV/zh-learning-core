"use client";

import {
    useCallback,
    useDeferredValue,
    useEffect,
    useMemo,
    useState,
} from "react";
import Link from "next/link";
import { FolderOpen, PenSquare, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { CreateCourseModal } from "@/modules/lesson/components/course/create-course-modal";
import { CourseStatusConfirmDialog } from "@/modules/lesson/components/course/course-status-confirm-dialog";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Badge } from "@/shared/components/ui/badge";
import { Switch } from "@/shared/components/ui/switch";
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
import { CustomPagination } from "@/shared/components/cms/custom-pagination";
import {
    useDeleteCourse,
    useGetListCourse,
    usePublishCourse,
    useUnPublishCourse,
} from "@/modules/lesson/hooks/use.course.tanstack";
import {
    CourseListQueryParams,
    CourseSortBy,
} from "@/modules/lesson/types/coure.type";

const initialCourseQueryParams: CourseListQueryParams = {
    title: "",
    orderByDescending: true,
    sortBy: "CreatedAt",
    take: 50,
};

const pageContainerClass = "mx-auto w-full max-w-[1400px] px-5";

export default function CourseCmsPage() {
    const [activeTab, setActiveTab] = useState<"courses" | "stats">("courses");
    const [itemsPerPage, setItemsPerPage] = useState(50);
    const [queryParams, setQueryParams] = useState<CourseListQueryParams>(
        initialCourseQueryParams,
    );
    const [levelFilter, setLevelFilter] = useState("all");
    const [publishFilter, setPublishFilter] = useState("all");
    const [currentPage, setCurrentPage] = useState(1);
    const [pendingCourseId, setPendingCourseId] = useState<string | null>(null);
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

    const deferredTitle = useDeferredValue(queryParams.title ?? "");

    const effectiveQueryParams = useMemo<CourseListQueryParams>(
        () => ({
            ...queryParams,
            title: deferredTitle.trim(),
            take: itemsPerPage,
        }),
        [queryParams, deferredTitle, itemsPerPage],
    );

    const coursesQuery = useGetListCourse(effectiveQueryParams);
    const publishCourseMutation = usePublishCourse();
    const unPublishCourseMutation = useUnPublishCourse();
    const deleteCourseMutation = useDeleteCourse();

    const pages = coursesQuery.data?.pages ?? [];
    const loadedPages = pages.length;
    const firstPageData = pages[0]?.data;
    const currentPageData = pages[currentPage - 1]?.data;
    const pageCourses = currentPageData?.items ?? [];
    const loadedCourses = useMemo(
        () => pages.flatMap((page) => page.data.items ?? []),
        [pages],
    );

    const canLoadMore =
        loadedPages > 0
            ? (pages[loadedPages - 1]?.data?.hasNextPage ?? false)
            : false;

    const filteredCourses = useMemo(() => {
        return pageCourses.filter((course) => {
            const matchLevel =
                levelFilter === "all" ||
                String(course.hskLevel) === levelFilter;

            const matchPublish =
                publishFilter === "all" ||
                (publishFilter === "published"
                    ? course.isPublished
                    : !course.isPublished);

            return matchLevel && matchPublish;
        });
    }, [pageCourses, levelFilter, publishFilter]);

    const totalDocs = firstPageData?.total ?? 0;
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

    const estimatedPages = loadedPages + (canLoadMore ? 1 : 0);
    const pagesFromTotal = Math.max(1, Math.ceil(totalDocs / itemsPerPage));
    const totalPages = Math.max(1, Math.min(estimatedPages, pagesFromTotal));
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = Math.min(startIndex + filteredCourses.length, totalDocs);
    const totalItems = totalDocs;

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
        levelFilter,
        publishFilter,
        itemsPerPage,
    ]);

    const resetFilters = () => {
        setQueryParams(initialCourseQueryParams);
        setLevelFilter("all");
        setPublishFilter("all");
        setCurrentPage(1);
    };

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

    const handleDeleteCourse = async (courseId: string, title: string) => {
        const confirmed = window.confirm(
            `Xóa khóa học \"${title}\"? Hành động này không thể hoàn tác.`,
        );
        if (!confirmed) return;

        setPendingCourseId(courseId);
        try {
            await deleteCourseMutation.mutateAsync(courseId);
            toast.success("Đã xóa khóa học.");
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setPendingCourseId(null);
        }
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
                !coursesQuery.isFetchingNextPage
            ) {
                void coursesQuery.fetchNextPage().then((result) => {
                    const fetchedPageCount =
                        result.data?.pages.length ?? loadedPages;
                    if (fetchedPageCount >= page) {
                        setCurrentPage(page);
                    }
                });
            }
        },
        [currentPage, loadedPages, canLoadMore, coursesQuery],
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
                            <div className="flex gap-1 rounded-lg bg-slate-100/60 p-1">
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
                                    <Button
                                        type="button"
                                        key={tab.id}
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
                                                : "text-slate-600 hover:text-slate-900"
                                        }`}
                                    >
                                        {tab.label}
                                        {tab.count !== null && (
                                            <span className="ml-2 rounded bg-slate-200 px-2 py-1 text-xs">
                                                {tab.count}
                                            </span>
                                        )}
                                    </Button>
                                ))}
                            </div>
                            <p className="text-xs text-slate-500 sm:text-sm">
                                Hiển thị {filteredCourses.length}/{totalDocs}{" "}
                                khóa học
                            </p>
                        </div>

                        <div className="mt-3 flex flex-wrap gap-3">
                            <Input
                                type="text"
                                value={queryParams.title || ""}
                                onChange={(event) =>
                                    setQueryParams({
                                        ...queryParams,
                                        title: event.target.value,
                                    })
                                }
                                placeholder="Tìm kiếm khóa học..."
                                className="h-9 min-w-64 flex-1 bg-white text-sm"
                            />

                            <Select
                                value={levelFilter}
                                onValueChange={(value) => setLevelFilter(value)}
                            >
                                <SelectTrigger className="h-9 w-32 bg-white text-sm">
                                    <SelectValue placeholder="HSK" />
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

                            <Select
                                value={publishFilter}
                                onValueChange={(value) =>
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
                                onValueChange={(value: CourseSortBy) =>
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
                    </div>
                </div>
            </div>

            <div className={`${pageContainerClass} min-w-0 pb-4 pt-3 lg:pb-5`}>
                {activeTab === "courses" && (
                    <div>
                        <div className="overflow-hidden rounded-xl border border-slate-200/50 bg-white shadow-sm">
                            <div className="max-h-[60vh] overflow-y-auto">
                                <Table>
                                    <TableHeader className="sticky top-0 z-10 bg-slate-50/95 backdrop-blur-sm">
                                        <TableRow className="border-b border-slate-200/50 hover:bg-slate-50/50">
                                            <TableHead className="w-16 px-4 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600">
                                                Index
                                            </TableHead>
                                            <TableHead className="px-4 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600">
                                                Tên khóa học
                                            </TableHead>
                                            <TableHead className="px-4 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600">
                                                Slug
                                            </TableHead>
                                            <TableHead className="px-4 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600">
                                                Hsk
                                            </TableHead>
                                            <TableHead className="px-4 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600">
                                                Tổng Học viên
                                            </TableHead>
                                            <TableHead className="px-4 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600">
                                                Tổng Topics
                                            </TableHead>
                                            <TableHead className="px-4 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600">
                                                Trạng thái
                                            </TableHead>
                                            <TableHead className="px-4 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600">
                                                Thao tác
                                            </TableHead>
                                        </TableRow>
                                    </TableHeader>

                                    <TableBody>
                                        {coursesQuery.isLoading && (
                                            <TableRow className="hover:bg-transparent">
                                                <TableCell
                                                    colSpan={9}
                                                    className="h-24 text-center text-sm text-slate-500"
                                                >
                                                    Đang tải danh sách khóa
                                                    học...
                                                </TableCell>
                                            </TableRow>
                                        )}

                                        {!coursesQuery.isLoading &&
                                            coursesQuery.isFetching && (
                                                <TableRow className="hover:bg-transparent">
                                                    <TableCell
                                                        colSpan={9}
                                                        className="h-12 text-center text-xs text-slate-500"
                                                    >
                                                        Đang đồng bộ dữ liệu
                                                        mới...
                                                    </TableCell>
                                                </TableRow>
                                            )}

                                        {coursesQuery.isError && (
                                            <TableRow className="hover:bg-transparent">
                                                <TableCell
                                                    colSpan={9}
                                                    className="h-24 text-center text-sm text-red-600"
                                                >
                                                    Không thể tải danh sách khóa
                                                    học.
                                                </TableCell>
                                            </TableRow>
                                        )}

                                        {filteredCourses.map((course) => (
                                            <TableRow
                                                key={course.id}
                                                className="border-b border-slate-100 hover:bg-slate-50/50"
                                            >
                                                <TableCell className="px-4 py-3 text-sm font-medium text-slate-500">
                                                    {course.orderIndex}
                                                </TableCell>
                                                <TableCell className="whitespace-normal px-4 py-3">
                                                    <p className="text-sm font-medium text-slate-900">
                                                        {course.title}
                                                    </p>
                                                </TableCell>
                                                <TableCell className="px-4 py-3 text-sm text-slate-600">
                                                    {course.slug}
                                                </TableCell>
                                                <TableCell className="px-4 py-3 text-sm text-slate-600">
                                                    {course.hskLevel}
                                                </TableCell>
                                                <TableCell className="px-4 py-3 text-sm font-medium text-slate-500">
                                                    {
                                                        course.totalStudentsEnrolled
                                                    }
                                                </TableCell>
                                                <TableCell className="px-4 py-3 text-sm font-medium text-slate-500">
                                                    {course.totalTopics}
                                                </TableCell>
                                                <TableCell className="px-4 py-3">
                                                    <div className="flex items-center gap-2">
                                                        <Switch
                                                            checked={
                                                                course.isPublished
                                                            }
                                                            size="default"
                                                            className="h-6 w-11 border border-slate-300 ring-1 ring-slate-200 data-checked:bg-emerald-600 data-unchecked:bg-slate-300"
                                                            disabled={
                                                                pendingCourseId ===
                                                                course.id
                                                            }
                                                            onCheckedChange={(
                                                                checked,
                                                            ) =>
                                                                handleOpenPublishDialog(
                                                                    course.id,
                                                                    course.title,
                                                                    checked,
                                                                )
                                                            }
                                                            aria-label={`Chuyển trạng thái xuất bản của ${course.title}`}
                                                        />
                                                        <Badge
                                                            className={`${
                                                                course.isPublished
                                                                    ? "bg-green-100 text-green-700"
                                                                    : "bg-slate-100 text-slate-600"
                                                            }`}
                                                        >
                                                            {course.isPublished
                                                                ? "Đã xuất bản"
                                                                : "Nháp"}
                                                        </Badge>
                                                    </div>
                                                </TableCell>
                                                <TableCell className="px-4 py-3">
                                                    <div className="flex gap-2">
                                                        <Tooltip>
                                                            <TooltipTrigger
                                                                asChild
                                                            >
                                                                <Button
                                                                    type="button"
                                                                    size="icon-sm"
                                                                    variant="outline"
                                                                    disabled={
                                                                        pendingCourseId ===
                                                                        course.id
                                                                    }
                                                                    onClick={() =>
                                                                        void handleDeleteCourse(
                                                                            course.id,
                                                                            course.title,
                                                                        )
                                                                    }
                                                                    className="text-rose-700 hover:text-rose-800"
                                                                >
                                                                    <Trash2 className="h-4 w-4" />
                                                                </Button>
                                                            </TooltipTrigger>
                                                            <TooltipContent>
                                                                Xóa khóa học
                                                            </TooltipContent>
                                                        </Tooltip>

                                                        <Tooltip>
                                                            <TooltipTrigger
                                                                asChild
                                                            >
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
                                                            <TooltipTrigger
                                                                asChild
                                                            >
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
                                                                Vào chi tiết
                                                                khóa
                                                            </TooltipContent>
                                                        </Tooltip>
                                                    </div>
                                                </TableCell>
                                            </TableRow>
                                        ))}

                                        {!coursesQuery.isLoading &&
                                            !coursesQuery.isError &&
                                            filteredCourses.length === 0 && (
                                                <TableRow className="hover:bg-transparent">
                                                    <TableCell
                                                        colSpan={9}
                                                        className="h-24 text-center text-sm text-slate-500"
                                                    >
                                                        Không có khóa học phù
                                                        hợp với bộ lọc hiện tại.
                                                    </TableCell>
                                                </TableRow>
                                            )}
                                    </TableBody>
                                </Table>
                            </div>
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
        </div>
    );
}
