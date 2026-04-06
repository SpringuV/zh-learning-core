"use client";

import { type UserListQueryParams } from "@/modules/users/api/users.api";
import {
    defaultFilters,
    type FilterState,
    UserManagementFilters,
} from "@/modules/users/components/user.management.filters";
import { useUserList } from "@/modules/users/hooks/use.user.query";
import { Button } from "@/shared/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/shared/components/ui/card";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/shared/components/ui/table";
import { cn } from "@/shared/lib/utils";
import { Filter } from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { ErrorAlert } from "@/shared/components/errors/error-alert";
import { CustomPagination } from "@/shared/components/cms/custom-pagination";

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
    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(50);
    const [draftFilters, setDraftFilters] =
        useState<FilterState>(defaultFilters);
    const [appliedFilters, setAppliedFilters] =
        useState<FilterState>(defaultFilters);

    const queryParams = useMemo<UserListQueryParams>(() => {
        const mappedIsActive =
            appliedFilters.isActive === "all"
                ? undefined
                : appliedFilters.isActive === "true";

        const sortBy =
            appliedFilters.sortMode === "az" || appliedFilters.sortMode === "za"
                ? "Username"
                : "CreatedAt";

        const orderByDescending =
            appliedFilters.sortMode === "za" ||
            appliedFilters.sortMode === "newest";

        return {
            email: appliedFilters.email,
            username: appliedFilters.username,
            phoneNumber: appliedFilters.phoneNumber,
            isActive: mappedIsActive,
            sortBy,
            orderByDescending,
            take: 150,
        };
    }, [appliedFilters]);

    const userManagementData = useUserList(queryParams);
    const users =
        userManagementData.data?.pages.flatMap((page) => page.data.items) ?? [];
    // pagination
    const totalDocs =
        userManagementData.data?.pages[0]?.data.total ?? users.length;
    const totalItems = totalDocs;
    const totalPages = Math.max(1, Math.ceil(totalDocs / itemsPerPage));
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    const paginatedUsers = users.slice(startIndex, endIndex);

    useEffect(() => {
        if (currentPage > totalPages) {
            setCurrentPage(totalPages);
        }
    }, [currentPage, totalPages]);

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

    const handlePageChange = useCallback(
        async (page: number) => {
            // Ensure the requested page is within bounds: ex: if totalPages is 5, clamp page to [1, 5]
            const safePage = Math.max(1, Math.min(page, totalPages));
            // Calculate how many items we need to have loaded to display the requested page
            const requiredItems = safePage * itemsPerPage;

            let loadedItems = users.length;
            let hasMore = Boolean(userManagementData.hasNextPage);

            while (loadedItems < requiredItems && hasMore) {
                const result = await userManagementData.fetchNextPage();
                const pages = result.data?.pages ?? [];
                loadedItems = pages.reduce(
                    (sum, current) => sum + current.data.items.length,
                    0,
                );
                hasMore = Boolean(result.hasNextPage);
            }

            setCurrentPage(safePage);
        },
        [
            totalPages,
            itemsPerPage,
            users.length,
            userManagementData.hasNextPage,
            userManagementData.fetchNextPage,
        ],
    );

    return (
        <div className="flex flex-1 flex-col gap-4">
            <Card className="py-0! gap-2">
                <CardHeader className="flex! flex-row! items-center! justify-between! gap-3 px-5! py-3!">
                    <div>
                        <CardTitle className="text-xl">
                            User Management
                        </CardTitle>
                        <CardDescription className="mt-1 text-sm">
                            Quản lý danh sách user, trạng thái kích hoạt và
                            thông tin cơ bản.
                        </CardDescription>
                    </div>

                    <Button
                        size="sm"
                        variant="outline"
                        onClick={() => setIsFilterVisible((prev) => !prev)}
                    >
                        <Filter className="size-4" />
                        Bộ lọc
                    </Button>
                </CardHeader>

                {isFilterVisible ? (
                    <CardContent className="border-t px-4 py-3">
                        <UserManagementFilters
                            value={draftFilters}
                            onChange={setDraftFilters}
                            onApply={handleApplyFilters}
                            onReset={handleResetFilters}
                        />
                    </CardContent>
                ) : null}
            </Card>

            <Card className="py-0 gap-2">
                <CardHeader className="flex! flex-row! items-center! justify-between! gap-2 border-b px-4! py-3!">
                    <CardTitle>Danh sách users</CardTitle>
                    <CardDescription>
                        Tổng:{" "}
                        {userManagementData.data?.pages[0]?.data.total ?? 0}{" "}
                        người dùng
                    </CardDescription>
                </CardHeader>

                <CardContent className="px-4 pb-4">
                    {userManagementData.isLoading ? (
                        <div className="flex h-40 items-center justify-center">
                            <div className="text-center">
                                <div className="mb-2 inline-block">
                                    <div className="h-8 w-8 animate-spin rounded-full border-4 border-muted border-t-primary"></div>
                                </div>
                                <p className="text-sm text-muted-foreground">
                                    Đang tải danh sách người dùng...
                                </p>
                            </div>
                        </div>
                    ) : null}

                    {userManagementData.isError ? (
                        <ErrorAlert
                            error={userManagementData.error}
                            title="Không thể tải danh sách người dùng"
                            onRetry={handleRetry}
                            isLoading={userManagementData.isLoading}
                        />
                    ) : null}

                    {!userManagementData.isLoading &&
                    !userManagementData.isError ? (
                        <div className="max-h-[58vh] overflow-y-auto">
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead className="uppercase">
                                            Email
                                        </TableHead>
                                        <TableHead className="uppercase">
                                            Username
                                        </TableHead>
                                        <TableHead className="uppercase">
                                            Phone
                                        </TableHead>
                                        <TableHead className="uppercase">
                                            Trạng thái
                                        </TableHead>
                                        <TableHead className="uppercase">
                                            Level
                                        </TableHead>
                                        <TableHead className="uppercase">
                                            Ngày tạo
                                        </TableHead>
                                        <TableHead className="uppercase">
                                            Cập nhật
                                        </TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {users.length > 0 ? (
                                        paginatedUsers.map((user) => (
                                            <TableRow key={user.id}>
                                                <TableCell className="font-medium">
                                                    {user.email}
                                                </TableCell>
                                                <TableCell>
                                                    {user.username}
                                                </TableCell>
                                                <TableCell>
                                                    {user.phoneNumber || "-"}
                                                </TableCell>
                                                <TableCell>
                                                    <span
                                                        className={cn(
                                                            "inline-flex rounded-full px-2 py-1 text-xs font-medium",
                                                            user.isActive
                                                                ? "bg-emerald-100 text-emerald-700"
                                                                : "bg-amber-100 text-amber-700",
                                                        )}
                                                    >
                                                        {user.isActive
                                                            ? "Đã kích hoạt"
                                                            : "Chưa kích hoạt"}
                                                    </span>
                                                </TableCell>
                                                <TableCell>
                                                    {user.currentLevel}
                                                </TableCell>
                                                <TableCell>
                                                    {formatDate(user.createdAt)}
                                                </TableCell>
                                                <TableCell>
                                                    {formatDate(user.updatedAt)}
                                                </TableCell>
                                            </TableRow>
                                        ))
                                    ) : (
                                        <TableRow>
                                            <TableCell
                                                colSpan={7}
                                                className="h-24 text-center text-muted-foreground"
                                            >
                                                Không có user phù hợp với bộ
                                                lọc.
                                            </TableCell>
                                        </TableRow>
                                    )}
                                </TableBody>
                            </Table>
                        </div>
                    ) : null}

                    {!userManagementData.isLoading &&
                    !userManagementData.isError &&
                    users.length > 0 ? (
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
                    ) : null}
                </CardContent>
            </Card>
        </div>
    );
};

export default UserManagementComponent;
