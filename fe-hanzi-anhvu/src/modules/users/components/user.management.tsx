"use client";

import {
    type UserListQueryParams,
    type UserSortBy,
} from "@/modules/users/api/users.api";
import { useUserList } from "@/modules/users/hooks/use.user.query";
import { Button } from "@/shared/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/shared/components/ui/card";
import { Input } from "@/shared/components/ui/input";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/shared/components/ui/table";
import { cn } from "@/shared/lib/utils";
import { Filter, RotateCcw } from "lucide-react";
import { useMemo, useState } from "react";
import { ErrorAlert } from "@/shared/components/errors/error-alert";

type FilterState = {
    email: string;
    username: string;
    phoneNumber: string;
    isActive: "all" | "true" | "false";
    sortBy: UserSortBy;
    startDate?: string;
    endDate?: string;
};

const defaultFilters: FilterState = {
    email: "",
    username: "",
    phoneNumber: "",
    isActive: "all",
    sortBy: "CreatedAt",
    startDate: undefined,
    endDate: undefined,
};

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
    const [draftFilters, setDraftFilters] =
        useState<FilterState>(defaultFilters);
    const [appliedFilters, setAppliedFilters] =
        useState<FilterState>(defaultFilters);

    const queryParams = useMemo<UserListQueryParams>(() => {
        const mappedIsActive =
            appliedFilters.isActive === "all"
                ? undefined
                : appliedFilters.isActive === "true";

        return {
            email: appliedFilters.email,
            username: appliedFilters.username,
            phoneNumber: appliedFilters.phoneNumber,
            isActive: mappedIsActive,
            sortBy: appliedFilters.sortBy,
            orderByDescending: true,
            take: 30,
        };
    }, [appliedFilters]);

    const userManagementData = useUserList(queryParams);
    const users = userManagementData.data?.data.items ?? [];

    const handleApplyFilters = () => {
        setAppliedFilters(draftFilters);
    };

    const handleResetFilters = () => {
        setDraftFilters(defaultFilters);
        setAppliedFilters(defaultFilters);
    };

    const handleRetry = () => {
        userManagementData.refetch();
    };

    return (
        <div className="flex flex-1 flex-col gap-6">
            <Card>
                <CardHeader className="flex flex-row items-start justify-between gap-4">
                    <div>
                        <CardTitle>User Management</CardTitle>
                        <CardDescription>
                            Quản lý danh sách user, trạng thái kích hoạt và
                            thông tin cơ bản.
                        </CardDescription>
                    </div>

                    <Button
                        variant="outline"
                        onClick={() => setIsFilterVisible((prev) => !prev)}
                    >
                        <Filter className="size-4" />
                        Bộ lọc
                    </Button>
                </CardHeader>

                {isFilterVisible ? (
                    <CardContent className="grid grid-cols-1 gap-3 border-t pt-6 md:grid-cols-2 xl:grid-cols-5">
                        <Input
                            placeholder="Lọc theo email"
                            value={draftFilters.email}
                            onChange={(event) =>
                                setDraftFilters((prev) => ({
                                    ...prev,
                                    email: event.target.value,
                                }))
                            }
                        />
                        <Input
                            placeholder="Lọc theo username"
                            value={draftFilters.username}
                            onChange={(event) =>
                                setDraftFilters((prev) => ({
                                    ...prev,
                                    username: event.target.value,
                                }))
                            }
                        />
                        <Input
                            placeholder="Lọc theo số điện thoại"
                            value={draftFilters.phoneNumber}
                            onChange={(event) =>
                                setDraftFilters((prev) => ({
                                    ...prev,
                                    phoneNumber: event.target.value,
                                }))
                            }
                        />

                        <select
                            className="h-9 rounded-md border border-input bg-background px-3 text-sm"
                            value={draftFilters.isActive}
                            onChange={(event) =>
                                setDraftFilters((prev) => ({
                                    ...prev,
                                    isActive: event.target
                                        .value as FilterState["isActive"],
                                }))
                            }
                        >
                            <option value="all">Tất cả trạng thái</option>
                            <option value="true">Đã kích hoạt</option>
                            <option value="false">Chưa kích hoạt</option>
                        </select>

                        <select
                            className="h-9 rounded-md border border-input bg-background px-3 text-sm"
                            value={draftFilters.sortBy}
                            onChange={(event) =>
                                setDraftFilters((prev) => ({
                                    ...prev,
                                    sortBy: event.target.value as UserSortBy,
                                }))
                            }
                        >
                            <option value="CreatedAt">Sắp xếp: Ngày tạo</option>
                            <option value="UpdatedAt">
                                Sắp xếp: Ngày cập nhật
                            </option>
                            <option value="CurrentLevel">
                                Sắp xếp: Cấp độ
                            </option>
                        </select>

                        <div className="col-span-1 flex gap-2 md:col-span-2 xl:col-span-5">
                            <Button onClick={handleApplyFilters}>
                                Áp dụng bộ lọc
                            </Button>
                            <Button
                                variant="outline"
                                onClick={handleResetFilters}
                            >
                                <RotateCcw className="size-4" />
                                Đặt lại
                            </Button>
                        </div>
                    </CardContent>
                ) : null}
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle>Danh sách users</CardTitle>
                    <CardDescription>
                        Tổng: {userManagementData.data?.data.total ?? 0} người
                        dùng
                    </CardDescription>
                </CardHeader>

                <CardContent>
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
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Email</TableHead>
                                    <TableHead>Username</TableHead>
                                    <TableHead>Phone</TableHead>
                                    <TableHead>Trạng thái</TableHead>
                                    <TableHead>Level</TableHead>
                                    <TableHead>Ngày tạo</TableHead>
                                    <TableHead>Cập nhật</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {users.length > 0 ? (
                                    users.map((user) => (
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
                                            Không có user phù hợp với bộ lọc.
                                        </TableCell>
                                    </TableRow>
                                )}
                            </TableBody>
                        </Table>
                    ) : null}
                </CardContent>
            </Card>
        </div>
    );
};

export default UserManagementComponent;
