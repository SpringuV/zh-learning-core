import LogoutComponent from "@/modules/auth/components/logout";
import {
    Avatar,
    AvatarFallback,
    AvatarImage,
} from "@/shared/components/ui/avatar";
import { Button } from "@/shared/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuGroup,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu";
import {
    BadgeCheck,
    Bell,
    Coffee,
    CreditCard,
    LayoutPanelLeft,
    Menu,
    Search,
    Shield,
} from "lucide-react";
import { useSession } from "next-auth/react";

const navItems = [{ label: "Mới Cafe", icon: Coffee, active: true }];

type ClientDashboardHeaderProps = {
    isSidebarCollapsed: boolean;
    onToggleSidebar: () => void;
};

export function ClientDashboardHeader({
    isSidebarCollapsed,
    onToggleSidebar,
}: ClientDashboardHeaderProps) {
    const { data: session } = useSession();

    const getInitials = (name: string) => {
        return name
            .split(" ")
            .map((n) => n[0])
            .join("")
            .toUpperCase();
    };

    const userName = session?.user?.name || "User";
    const userImage = session?.user?.image || "";
    const userEmail = session?.user?.email || "";

    return (
        <header className="flex h-16 items-center gap-4 border-b border-slate-200 bg-white px-4 md:px-6">
            <div className="hidden items-center gap-3 lg:flex">
                <button
                    type="button"
                    onClick={onToggleSidebar}
                    className={
                        "grid size-8 place-items-center rounded-md border border-slate-200 transition-colors " +
                        (isSidebarCollapsed
                            ? "bg-emerald-100 text-emerald-700"
                            : "text-slate-600 hover:bg-slate-100")
                    }
                    aria-label="Toggle sidebar"
                    title="Thu gọn/mở rộng sidebar"
                >
                    <LayoutPanelLeft className="size-4" />
                </button>
            </div>

            <button className="grid size-9 place-items-center rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-100 lg:hidden">
                <Menu className="size-4" />
            </button>

            <label className="flex h-11 min-w-0 flex-1 items-center gap-3 rounded-xl border border-slate-200 bg-slate-50 px-3 text-slate-500">
                <Search className="size-4 shrink-0" />
                <input
                    type="text"
                    placeholder="Tìm khóa học..."
                    className="w-full bg-transparent text-xs text-slate-800 outline-none placeholder:text-slate-400 md:text-sm"
                />
            </label>

            <nav className="hidden items-center gap-6 lg:flex">
                {navItems.map((item) => {
                    const Icon = item.icon;
                    return (
                        <button
                            key={item.label}
                            className={
                                "flex items-center gap-2 text-sm font-semibold transition-colors " +
                                (item.active
                                    ? "text-emerald-700"
                                    : "text-slate-600 hover:text-slate-900")
                            }
                        >
                            <Icon className="size-4" />
                            {item.label}
                        </button>
                    );
                })}
            </nav>

            <div className="flex items-center gap-2">
                <button className="relative grid size-9 place-items-center rounded-full border border-slate-200 text-slate-600 hover:bg-slate-100">
                    <Bell className="size-4" />
                    <span className="absolute -right-1 -top-1 size-2.5 rounded-full bg-emerald-500" />
                </button>
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button
                            variant="ghost"
                            size="icon"
                            className="rounded-full border border-transparent bg-transparent p-0 shadow-none hover:bg-slate-100"
                        >
                            <Avatar className="h-8 w-8 rounded-full ring-2 ring-emerald-200 after:border-transparent">
                                <AvatarImage src={userImage} alt={userName} />
                                <AvatarFallback className="rounded-full bg-emerald-100 text-emerald-700">
                                    {getInitials(userName)}
                                </AvatarFallback>
                            </Avatar>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent
                        className="w-72 max-w-[calc(100vw-24px)] rounded-lg"
                        side="bottom"
                        align="end"
                        sideOffset={10}
                        collisionPadding={12}
                    >
                        <DropdownMenuLabel className="p-0 font-normal">
                            <div className="flex items-center gap-2 px-1 py-1.5 text-left text-sm">
                                <Avatar className="h-8 w-8 rounded-full after:border-transparent">
                                    <AvatarImage
                                        src={userImage}
                                        alt={userName}
                                    />
                                    <AvatarFallback className="rounded-full bg-emerald-100 text-emerald-700">
                                        {getInitials(userName)}
                                    </AvatarFallback>
                                </Avatar>
                                <div className="grid min-w-0 flex-1 text-left text-sm leading-tight">
                                    <span className="truncate font-medium">
                                        {userName}
                                    </span>
                                    {userEmail ? (
                                        <span className="truncate text-xs text-slate-500">
                                            {userEmail}
                                        </span>
                                    ) : null}
                                </div>
                            </div>
                        </DropdownMenuLabel>
                        <DropdownMenuSeparator />
                        {session?.user?.roles?.includes("Administrators") && (
                            <DropdownMenuGroup>
                                <DropdownMenuItem>
                                    <Shield className="size-4" />
                                    Admin Panel
                                </DropdownMenuItem>
                            </DropdownMenuGroup>
                        )}
                        <DropdownMenuSeparator />
                        <DropdownMenuGroup>
                            <DropdownMenuItem>
                                <BadgeCheck className="size-4" />
                                Tài khoản
                            </DropdownMenuItem>
                            <DropdownMenuItem>
                                <CreditCard className="size-4" />
                                Đơn hàng
                            </DropdownMenuItem>
                            <DropdownMenuItem>
                                <Bell className="size-4" />
                                Thông báo
                            </DropdownMenuItem>
                        </DropdownMenuGroup>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem>
                            <LogoutComponent />
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
        </header>
    );
}
