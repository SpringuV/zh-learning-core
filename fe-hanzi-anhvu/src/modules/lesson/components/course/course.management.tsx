"use client";

import {
    useCallback,
    useDeferredValue,
    useEffect,
    useMemo,
    useState,
} from "react";
import Link from "next/link";
import { Eye, EyeOff, FolderOpen, PenSquare, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { CreateCourseModal } from "@/modules/lesson/components/course/create-course-modal";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Badge } from "@/shared/components/ui/badge";
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

export default function CourseCmsPage() {
    // #region State
    const [activeTab, setActiveTab] = useState<"courses" | "stats">("courses");
    const [itemsPerPage, setItemsPerPage] = useState(50);
    const [queryParams, setQueryParams] = useState<CourseListQueryParams>(
        initialCourseQueryParams,
    );
    const [levelFilter, setLevelFilter] = useState("all");
    const [publishFilter, setPublishFilter] = useState("all");
    const [currentPage, setCurrentPage] = useState(1);
    const [pendingCourseId, setPendingCourseId] = useState<string | null>(null);
    // #endregion
    // deferredValue để trì hoãn việc cập nhật giá trị tìm kiếm khi người dùng đang gõ, giúp giảm số lần gọi API khi người dùng nhập vào ô tìm kiếm, chỉ cập nhật giá trị tìm kiếm sau khi người dùng ngừng gõ một khoảng thời gian ngắn
    const deferredTitle = useDeferredValue(queryParams.title ?? "");

    // Memoized effective query params to avoid unnecessary refetches
    const effectiveQueryParams = useMemo<CourseListQueryParams>(
        () => ({
            ...queryParams,
            title: deferredTitle.trim(),
            take: itemsPerPage,
        }),
        [queryParams, deferredTitle, itemsPerPage],
    );

    // #region Data Fetching
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
    //#endregion

    // #region Effects and Callbacks
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

    const handleTogglePublishCourse = async (
        courseId: string,
        isPublished: boolean,
    ) => {
        const nextActionText = isPublished ? "hủy xuất bản" : "xuất bản";
        const confirmed = window.confirm(
            `Bạn có chắc muốn ${nextActionText} khóa học này không?`,
        );
        if (!confirmed) return;

        setPendingCourseId(courseId);
        try {
            if (isPublished) {
                await unPublishCourseMutation.mutateAsync(courseId);
                toast.success("Đã hủy xuất bản khóa học.");
            } else {
                await publishCourseMutation.mutateAsync(courseId);
                toast.success("Đã xuất bản khóa học.");
            }
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
    // #endregion
    return (
        <div className="bg-linear-to-br from-slate-50 via-white to-slate-50">
            {/* Top Navigation Bar */}
            <div className="sticky top-0 z-40 border-b border-slate-200/50 bg-white/80 backdrop-blur-lg">
                <div className="w-full px-5 py-4">
                    <div className="flex items-center justify-between">
                        <div>
                            <h1 className="text-3xl font-light tracking-tight">
                                <span className="text-slate-900">Quản lý</span>
                                <span className="text-amber-600">
                                    {" "}
                                    khóa học
                                </span>
                            </h1>
                            <p className="text-slate-500 text-sm mt-0.5">
                                Tạo và quản lý khóa học, chủ đề và bài tập
                            </p>
                        </div>
                        <CreateCourseModal />
                    </div>
                </div>
            </div>

            {/* Main Content */}
            <div className="w-full min-w-0">
                {/* Tabs */}
                <div className="mb-5 flex items-center justify-between">
                    <div className="flex gap-1 bg-slate-100/50 p-1 rounded-lg w-fit">
                        {[
                            {
                                id: "courses",
                                label: "Khóa học",
                                count: totalCourses,
                            },
                            { id: "stats", label: "Thống kê", count: null },
                        ].map((tab) => (
                            <Button
                                type="button"
                                key={tab.id}
                                onClick={() => setActiveTab(tab.id as any)}
                                variant={
                                    activeTab === tab.id ? "secondary" : "ghost"
                                }
                                size="sm"
                                className={`rounded-md font-medium transition-all duration-300 text-sm ${
                                    activeTab === tab.id
                                        ? "bg-white text-slate-900 shadow-sm"
                                        : "text-slate-600 hover:text-slate-900"
                                }`}
                            >
                                {tab.label}
                                {tab.count !== null && (
                                    <span className="ml-2 text-xs bg-slate-200 px-2 py-1 rounded">
                                        {tab.count}
                                    </span>
                                )}
                            </Button>
                        ))}
                    </div>
                    <div className="mb-4 flex flex-wrap items-center justify-end gap-3">
                        <div className="flex flex-wrap gap-3">
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
                                className="h-9 w-72 bg-white text-sm"
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
                                onValueChange={(value: CourseSortBy) =>
                                    setQueryParams((current) => ({
                                        ...current,
                                        sortBy: value,
                                    }))
                                }
                            >
                                <SelectTrigger className="h-9 w-44 bg-white text-sm">
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
                                className="h-9"
                                onClick={resetFilters}
                            >
                                Xóa lọc
                            </Button>
                        </div>
                    </div>
                </div>

                {/* Courses Tab */}
                {activeTab === "courses" && (
                    <div>
                        {/* Table View */}
                        <div className="bg-white rounded-xl border border-slate-200/50 overflow-hidden shadow-sm">
                            <Table>
                                <TableHeader className="bg-slate-50/50">
                                    <TableRow className="border-b border-slate-200/50 hover:bg-slate-50/50">
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider w-16">
                                            Index
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Tên khóa học
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Slug
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Hsk
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Tổng Học viên
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Tổng Topics
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                            Trạng thái
                                        </TableHead>
                                        <TableHead className="px-4 py-3 text-xs font-semibold text-slate-600 uppercase tracking-wider">
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
                                                Đang tải danh sách khóa học...
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
                                            <TableCell className="px-4 py-3 whitespace-normal">
                                                <div>
                                                    <p className="font-medium text-slate-900 text-sm">
                                                        {course.title}
                                                    </p>
                                                </div>
                                            </TableCell>
                                            <TableCell className="px-4 py-3 text-sm text-slate-600">
                                                {course.slug}
                                            </TableCell>
                                            <TableCell className="px-4 py-3 text-sm text-slate-600">
                                                {course.hskLevel}
                                            </TableCell>
                                            <TableCell className="px-4 py-3 text-sm font-medium text-slate-500">
                                                {course.totalStudentsEnrolled}
                                            </TableCell>
                                            <TableCell className="px-4 py-3 text-sm font-medium text-slate-500">
                                                {course.totalTopics}
                                            </TableCell>
                                            <TableCell className="px-4 py-3">
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
                                            </TableCell>
                                            <TableCell className="px-4 py-3">
                                                {pendingCourseId ===
                                                    course.id && (
                                                    <p className="mb-2 text-[11px] text-slate-500">
                                                        Đang xử lý...
                                                    </p>
                                                )}
                                                <div className="flex gap-2">
                                                    <Tooltip>
                                                        <TooltipTrigger asChild>
                                                            <Button
                                                                type="button"
                                                                size="icon-sm"
                                                                variant="outline"
                                                                disabled={
                                                                    pendingCourseId ===
                                                                    course.id
                                                                }
                                                                onClick={() =>
                                                                    void handleTogglePublishCourse(
                                                                        course.id,
                                                                        course.isPublished,
                                                                    )
                                                                }
                                                                className={
                                                                    course.isPublished
                                                                        ? "text-amber-700 hover:text-amber-800"
                                                                        : "text-emerald-700 hover:text-emerald-800"
                                                                }
                                                            >
                                                                {course.isPublished ? (
                                                                    <EyeOff className="h-4 w-4" />
                                                                ) : (
                                                                    <Eye className="h-4 w-4" />
                                                                )}
                                                            </Button>
                                                        </TooltipTrigger>
                                                        <TooltipContent>
                                                            {course.isPublished
                                                                ? "Hủy xuất bản"
                                                                : "Xuất bản"}
                                                        </TooltipContent>
                                                    </Tooltip>

                                                    <Tooltip>
                                                        <TooltipTrigger asChild>
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
                                                        <TooltipTrigger asChild>
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
                                                        <TooltipTrigger asChild>
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
                                                            Vào chi tiết khóa
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
                                                    Không có khóa học phù hợp
                                                    với bộ lọc hiện tại.
                                                </TableCell>
                                            </TableRow>
                                        )}
                                </TableBody>
                            </Table>
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

                {/* Statistics Tab */}
                {activeTab === "stats" && (
                    <div className="space-y-4">
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
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
                                    className="bg-white rounded-xl border border-slate-200/50 p-5"
                                >
                                    <p className="text-sm text-slate-600 font-medium mb-2">
                                        {stat.label}
                                    </p>
                                    <p
                                        className={`text-3xl font-light mb-1 ${stat.tone}`}
                                    >
                                        {stat.value}
                                    </p>
                                </div>
                            ))}
                        </div>

                        <div className="bg-white rounded-xl border border-slate-200/50 p-5">
                            <p className="text-sm text-slate-600 font-medium mb-2">
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
        </div>
    );
}
