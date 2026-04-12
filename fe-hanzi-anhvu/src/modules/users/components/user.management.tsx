"use client";

import { type UserListQueryParams } from "@/modules/users/api/users.api";
import {
    defaultFilters,
    type FilterState,
    UserManagementFilters,
} from "@/modules/users/components/user.management.filters";
import { useInfiniteUserList } from "@/modules/users/hooks/use.user.query";
import { cn } from "@/shared/lib/utils";
import { Filter } from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { ErrorAlert } from "@/shared/components/errors/error-alert";
import { CustomPagination } from "@/shared/components/cms/custom-pagination";
import { Badge } from "@/shared/components/ui/badge";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/shared/components/ui/table";

const formatDate = (value: string) => {
    return new Date(value).toLocaleString("vi-VN", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
    });
};

const UserManagementComponent = () => {
    const [isFilterVisible, setIsFilterVisible] = useState(false);
    const [itemsPerPage, setItemsPerPage] = useState(50);
    const [draftFilters, setDraftFilters] =
        useState<FilterState>(defaultFilters);
    const [appliedFilters, setAppliedFilters] =
        useState<FilterState>(defaultFilters);

    // Keep only current page state, cursor chain is managed by useInfiniteQuery.
    const [currentPage, setCurrentPage] = useState(1);

    const queryParams = useMemo<
        Omit<UserListQueryParams, "searchAfterValues">
    >(() => {
        const mappedIsActive =
            appliedFilters.isActive === "all"
                ? undefined
                : appliedFilters.isActive === "true";

        const orderByDescending = appliedFilters.orderBy === "desc";

        return {
            email: appliedFilters.email,
            username: appliedFilters.username,
            phoneNumber: appliedFilters.phoneNumber,
            isActive: mappedIsActive,
            sortBy: appliedFilters.sortBy,
            orderByDescending,
            take: itemsPerPage,
        };
    }, [appliedFilters, itemsPerPage]);

    const userManagementData = useInfiniteUserList(queryParams);
    const pages = userManagementData.data?.pages ?? [];
    const loadedPages = pages.length;
    const firstPageData = pages[0]?.data;
    const currentPageData = pages[currentPage - 1]?.data;
    const users = currentPageData?.items ?? [];
    const totalDocs = firstPageData?.total ?? 0;
    const canLoadMore =
        loadedPages > 0
            ? (pages[loadedPages - 1]?.data?.hasNextPage ?? false)
            : false;

    const handleApplyFilters = () => {
        setAppliedFilters(draftFilters);
        setCurrentPage(1);
    };

    const handleResetFilters = () => {
        setDraftFilters(defaultFilters);
        setAppliedFilters(defaultFilters);
        setCurrentPage(1);
    };

    const handleRetry = () => {
        userManagementData.refetch();
    };

    // Compute pagination values for CustomPagination component
    // With infinite query, total pages = loaded pages + (one virtual next page when hasNextPage=true).
    const estimatedPages = loadedPages + (canLoadMore ? 1 : 0);
    const pagesFromTotal = Math.max(1, Math.ceil(totalDocs / itemsPerPage));
    const totalPages = Math.max(1, Math.min(estimatedPages, pagesFromTotal));
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = Math.min(startIndex + users.length, totalDocs);
    const totalItems = totalDocs;

    useEffect(() => {
        if (currentPage > totalPages) {
            setCurrentPage(totalPages);
        }
    }, [currentPage, totalPages]);

    const handlePageChange = useCallback(
        (page: number) => {
            // Validate page number
            if (page < 1 || page === currentPage) return;

            // Page already loaded -> switch immediately.
            if (page <= loadedPages) {
                setCurrentPage(page);
                return;
            }

            // Cursor-based pagination can only fetch one next page at a time.
            if (
                page === loadedPages + 1 &&
                canLoadMore &&
                !userManagementData.isFetchingNextPage
            ) {
                void userManagementData.fetchNextPage().then((result) => {
                    const fetchedPageCount =
                        result.data?.pages.length ?? loadedPages;
                    if (fetchedPageCount >= page) {
                        setCurrentPage(page);
                    }
                });
            }
        },
        [currentPage, loadedPages, canLoadMore, userManagementData],
    );

    return (
        // via-white is too heavy for the background, it makes the content look like it's floating and hard to read, so I changed it to a more subtle gradient
        <div className=" min-h-screen bg-linear-to-br from-slate-50 via-white to-slate-50">
            {/* Top Navigation Bar */}
            <div // Bắt đầu với opacity 0 và hơi dịch xuống dưới, di chuyển dưới lên trên} // Kết thúc với opacity 1 và vị trí ban đầu}
                className="px-2 py-1 border-b border-slate-200/50 bg-white/80 backdrop-blur-lg flex flex-col md:flex-row items-start md:items-center justify-between gap-4"
            >
                <div>
                    <h1 className="text-2xl font-light tracking-tight">
                        <span className="text-slate-900">Quản lý</span>
                        <span className="text-amber-600"> Người dùng</span>
                    </h1>
                    <p className="text-slate-500 text-sm mt-1">
                        Quản lý danh sách user, trạng thái kích hoạt và thông
                        tin cơ bản.
                    </p>
                </div>
                <button
                    onClick={() => setIsFilterVisible((prev) => !prev)}
                    className="px-3 py-1 bg-white border border-slate-200 text-slate-700 rounded-lg hover:bg-slate-50 transition-colors font-medium flex items-center justify-center gap-2"
                >
                    <Filter className="w-4 h-4" />
                    Bộ lọc
                </button>
            </div>

            {/* Main Content */}
            <div className="w-full flex flex-col items-center-safe justify-center-safe py-3">
                {isFilterVisible && (
                    <div className="mb-8">
                        <div className="bg-white border border-slate-200/50 rounded-xl p-6 shadow-sm">
                            <UserManagementFilters
                                value={draftFilters}
                                onChange={setDraftFilters}
                                onApply={handleApplyFilters}
                                onReset={handleResetFilters}
                            />
                        </div>
                    </div>
                )}

                <div>
                    <div className="bg-white rounded-xl border border-slate-200/50 overflow-hidden shadow-sm">
                        <div className="px-6 py-4 border-b border-slate-200/50 bg-slate-50/50 flex justify-between items-center">
                            <h2 className="font-semibold text-slate-800">
                                Danh sách users
                            </h2>
                            <span className="text-sm text-slate-500">
                                Tổng: {totalDocs ?? 0} người dùng
                            </span>
                        </div>

                        {userManagementData.isLoading ? (
                            <div className="flex h-40 items-center justify-center">
                                <div className="text-center">
                                    <div className="mb-2 inline-block">
                                        <div className="h-8 w-8 animate-spin rounded-full border-4 border-slate-200 border-t-amber-600"></div>
                                    </div>
                                    <p className="text-sm text-slate-500">
                                        Đang tải danh sách người dùng...
                                    </p>
                                </div>
                            </div>
                        ) : userManagementData.isError ? (
                            <div className="p-6">
                                <ErrorAlert
                                    error={userManagementData.error}
                                    title="Không thể tải danh sách người dùng"
                                    onRetry={handleRetry}
                                    isLoading={userManagementData.isLoading}
                                />
                            </div>
                        ) : (
                            <div className="max-h-[58vh] max-w-[80vw] overflow-auto">
                                <Table>
                                    <TableHeader>
                                        <TableRow className="border-b border-slate-200/50 bg-slate-50/50 sticky top-0 z-10 backdrop-blur-md">
                                            <TableHead className="px-6 py-4 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                                Email
                                            </TableHead>
                                            <TableHead className="px-6 py-4 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                                Username
                                            </TableHead>
                                            <TableHead className="px-6 py-4 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                                Phone
                                            </TableHead>
                                            <TableHead className="px-6 py-4 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                                Trạng thái
                                            </TableHead>
                                            <TableHead className="px-6 py-4 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                                Level
                                            </TableHead>
                                            <TableHead className="px-6 py-4 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                                Ngày tạo
                                            </TableHead>
                                            <TableHead className="px-6 py-4 text-xs font-semibold text-slate-600 uppercase tracking-wider">
                                                Cập nhật
                                            </TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {users.length > 0 ? (
                                            users.map((user, idx) => (
                                                <tr
                                                    key={user.id}
                                                    className="border-b border-slate-100 hover:bg-slate-50/50 transition-colors"
                                                >
                                                    <TableCell className="px-6 py-4">
                                                        <span className="font-medium text-slate-900">
                                                            {user.email}
                                                        </span>
                                                    </TableCell>
                                                    <TableCell className="px-6 py-4 text-slate-600">
                                                        {user.username}
                                                    </TableCell>
                                                    <TableCell className="px-6 py-4 text-slate-600">
                                                        {user.phoneNumber ||
                                                            "-"}
                                                    </TableCell>
                                                    <TableCell className="px-6 py-4">
                                                        <Badge
                                                            variant="secondary"
                                                            className={cn(
                                                                "font-medium border-0 px-2 py-1",
                                                                user.isActive
                                                                    ? "bg-emerald-100 text-emerald-700 hover:bg-emerald-100/80"
                                                                    : "bg-amber-100 text-amber-700 hover:bg-amber-100/80",
                                                            )}
                                                        >
                                                            {user.isActive
                                                                ? "Đã kích hoạt"
                                                                : "Chưa kích hoạt"}
                                                        </Badge>
                                                    </TableCell>
                                                    <TableCell className="px-6 py-4 text-slate-600">
                                                        <Badge
                                                            variant="outline"
                                                            className="bg-slate-100 text-slate-700 hover:bg-slate-200 border-0 px-2 py-1"
                                                        >
                                                            Lvl{" "}
                                                            {user.currentLevel}
                                                        </Badge>
                                                    </TableCell>
                                                    <TableCell className="px-6 py-4 text-slate-500 text-sm">
                                                        {formatDate(
                                                            user.createdAt,
                                                        )}
                                                    </TableCell>
                                                    <TableCell className="px-6 py-4 text-slate-500 text-sm">
                                                        {formatDate(
                                                            user.updatedAt,
                                                        )}
                                                    </TableCell>
                                                </tr>
                                            ))
                                        ) : (
                                            <TableRow>
                                                <TableCell
                                                    colSpan={7}
                                                    className="px-6 py-12 text-center text-slate-500"
                                                >
                                                    Không có user phù hợp với bộ
                                                    lọc.
                                                </TableCell>
                                            </TableRow>
                                        )}
                                    </TableBody>
                                </Table>
                            </div>
                        )}

                        {!userManagementData.isLoading &&
                            !userManagementData.isError &&
                            users.length > 0 && (
                                <div className="p-4 border-t border-slate-200/50 bg-white">
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
                                </div>
                            )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default UserManagementComponent;
