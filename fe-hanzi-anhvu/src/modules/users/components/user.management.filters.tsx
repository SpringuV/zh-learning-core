import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/components/ui/select";
import { RotateCcw } from "lucide-react";

export type FilterState = {
    email: string;
    username: string;
    phoneNumber: string;
    isActive: "all" | "true" | "false";
    sortBy: "Email" | "Username" | "CreatedAt" | "UpdatedAt" | "CurrentLevel";
    orderBy: "asc" | "desc";
};

export const defaultFilters: FilterState = {
    email: "",
    username: "",
    phoneNumber: "",
    isActive: "all",
    sortBy: "CreatedAt",
    orderBy: "desc",
};

type UserManagementFiltersProps = {
    value: FilterState;
    onChange: (value: FilterState) => void;
    onApply: () => void;
    onReset: () => void;
};

export const UserManagementFilters = ({
    value,
    onChange,
    onApply,
    onReset,
}: UserManagementFiltersProps) => {
    return (
        <div className="grid grid-cols-1 gap-3 md:grid-cols-3 xl:grid-cols-12">
            <div className="space-y-1.5 xl:col-span-4">
                <Label
                    htmlFor="filter-email"
                    className="text-xs font-medium text-muted-foreground"
                >
                    Email
                </Label>
                <Input
                    id="filter-email"
                    placeholder="Lọc theo email"
                    className="h-8"
                    value={value.email}
                    onChange={(event) =>
                        onChange({
                            ...value,
                            email: event.target.value,
                        })
                    }
                />
            </div>

            <div className="space-y-1.5 xl:col-span-4">
                <Label
                    htmlFor="filter-username"
                    className="text-xs font-medium text-muted-foreground"
                >
                    Username
                </Label>
                <Input
                    id="filter-username"
                    placeholder="Lọc theo username"
                    className="h-8"
                    value={value.username}
                    onChange={(event) =>
                        onChange({
                            ...value,
                            username: event.target.value,
                        })
                    }
                />
            </div>

            <div className="space-y-1.5 xl:col-span-4">
                <Label
                    htmlFor="filter-phone"
                    className="text-xs font-medium text-muted-foreground"
                >
                    Số điện thoại
                </Label>
                <Input
                    id="filter-phone"
                    placeholder="Lọc theo số điện thoại"
                    className="h-8"
                    value={value.phoneNumber}
                    onChange={(event) =>
                        onChange({
                            ...value,
                            phoneNumber: event.target.value,
                        })
                    }
                />
            </div>

            <div className="space-y-1.5 xl:col-span-3">
                <Label
                    htmlFor="filter-status"
                    className="text-xs font-medium text-muted-foreground"
                >
                    Trạng thái
                </Label>
                <Select
                    value={value.isActive}
                    onValueChange={(isActive: FilterState["isActive"]) =>
                        onChange({
                            ...value,
                            isActive,
                        })
                    }
                >
                    <SelectTrigger id="filter-status" className="h-8">
                        <SelectValue placeholder="Tất cả trạng thái" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="all">Tất cả trạng thái</SelectItem>
                        <SelectItem value="true">Đã kích hoạt</SelectItem>
                        <SelectItem value="false">Chưa kích hoạt</SelectItem>
                    </SelectContent>
                </Select>
            </div>

            <div className="space-y-1.5 xl:col-span-3">
                <Label
                    htmlFor="filter-sort-by"
                    className="text-xs font-medium text-muted-foreground"
                >
                    Trường sắp xếp
                </Label>
                <Select
                    value={value.sortBy}
                    onValueChange={(sortBy: FilterState["sortBy"]) =>
                        onChange({
                            ...value,
                            sortBy,
                        })
                    }
                >
                    <SelectTrigger id="filter-sort-by" className="h-8">
                        <SelectValue placeholder="Chọn trường" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="CreatedAt">Ngày tạo</SelectItem>
                        <SelectItem value="UpdatedAt">Ngày cập nhật</SelectItem>
                        <SelectItem value="Email">Email</SelectItem>
                        <SelectItem value="Username">Username</SelectItem>
                        <SelectItem value="CurrentLevel">Level</SelectItem>
                    </SelectContent>
                </Select>
            </div>

            <div className="space-y-1.5 xl:col-span-3">
                <Label
                    htmlFor="filter-order-by"
                    className="text-xs font-medium text-muted-foreground"
                >
                    Thứ tự
                </Label>
                <Select
                    value={value.orderBy}
                    onValueChange={(orderBy: FilterState["orderBy"]) =>
                        onChange({
                            ...value,
                            orderBy,
                        })
                    }
                >
                    <SelectTrigger id="filter-order-by" className="h-8">
                        <SelectValue placeholder="Chọn thứ tự" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="asc">Tăng dần (A-Z, 1-9)</SelectItem>
                        <SelectItem value="desc">
                            Giảm dần (Z-A, 9-1)
                        </SelectItem>
                    </SelectContent>
                </Select>
            </div>

            <div className="col-span-1 flex flex-wrap items-end justify-start gap-2 pt-1 xl:col-span-12 xl:justify-end">
                <Button size="sm" onClick={onApply}>
                    Áp dụng bộ lọc
                </Button>
                <Button size="sm" variant="outline" onClick={onReset}>
                    <RotateCcw className="size-4" />
                    Đặt lại
                </Button>
            </div>
        </div>
    );
};
