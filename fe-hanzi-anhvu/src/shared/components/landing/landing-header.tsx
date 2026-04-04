"use client";
import Link from "next/link";
import {
    Send,
    SunMedium,
    Sparkles,
    BadgeCheck,
    CreditCard,
    Bell,
    LogOut,
    Shield,
} from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
    Avatar,
    AvatarImage,
    AvatarFallback,
} from "@/shared/components/ui/avatar";
import {
    DropdownMenu,
    DropdownMenuTrigger,
    DropdownMenuContent,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuGroup,
    DropdownMenuItem,
} from "@/shared/components/ui/dropdown-menu";
import { useSession } from "next-auth/react";
import LogoutComponent from "@/modules/auth/components/logout";

export function LandingHeader() {
    const { data: session } = useSession();
    // Get user initials for avatar fallback
    const getInitials = (name: string) => {
        return name
            .split(" ")
            .map((n) => n[0])
            .join("")
            .toUpperCase();
    };

    return (
        <header className="flex items-center justify-between rounded-2xl border border-white/20 bg-white/88 px-4 py-3 text-[#1f2a37] shadow-lg backdrop-blur md:px-6">
            <Link href="/" className="flex items-center gap-2">
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-white">
                    汉
                </div>
                <div>
                    <p className="text-lg font-bold">HanZi</p>
                    <p className="text-xs text-slate-500">
                        Học tiếng Trung hiệu quả, dễ dàng hơn
                    </p>
                </div>
            </Link>

            <nav className="hidden items-center gap-7 text-sm font-semibold lg:flex">
                <Link href="#features" className="hover:text-primary">
                    Khóa học
                </Link>
                <Link href="#review" className="hover:text-primary">
                    Review
                </Link>
                <Link href="#zoom" className="hover:text-primary">
                    Zoom Trực Tiếp
                </Link>
                <Link href="#roadmap" className="hover:text-primary">
                    Lộ Trình
                </Link>
            </nav>

            <div className="flex items-center gap-2">
                <Button
                    variant="ghost"
                    size="icon-sm"
                    className="text-[#0369a1]"
                >
                    <Send className="size-4" />
                </Button>
                <Button
                    variant="ghost"
                    size="icon-sm"
                    className="text-[#ea580c]"
                >
                    <SunMedium className="size-4" />
                </Button>
                {session?.user ? (
                    // User logged in - show dropdown menu
                    <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                            <Button
                                variant="ghost"
                                size="icon"
                                className="hover:bg-white/50"
                            >
                                <Avatar className="h-8 w-8 rounded-lg">
                                    <AvatarImage
                                        src={session.user.image || ""}
                                        alt={session.user.name || "User"}
                                    />
                                    <AvatarFallback className="rounded-lg">
                                        {getInitials(
                                            session.user.name || "User",
                                        )}
                                    </AvatarFallback>
                                </Avatar>
                            </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent
                            className="w-56 rounded-lg"
                            side="bottom"
                            align="end"
                            sideOffset={8}
                        >
                            <DropdownMenuLabel className="p-0 font-normal">
                                <div className="flex items-center gap-2 px-1 py-1.5 text-left text-sm">
                                    <Avatar className="h-8 w-8 rounded-lg">
                                        <AvatarImage
                                            src={session.user.image || ""}
                                            alt={session.user.name || "User"}
                                        />
                                        <AvatarFallback className="rounded-lg">
                                            {getInitials(
                                                session.user.name || "User",
                                            )}
                                        </AvatarFallback>
                                    </Avatar>
                                    <div className="grid flex-1 text-left text-sm leading-tight">
                                        <span className="truncate font-medium">
                                            {session.user.name}
                                        </span>
                                    </div>
                                </div>
                            </DropdownMenuLabel>
                            <DropdownMenuSeparator />
                            {session.user.roles?.includes("Administrators") && (
                                <DropdownMenuGroup>
                                    <DropdownMenuItem>
                                        <Shield className="size-4" />

                                        <Link href="/cms/dashboard">
                                            Admin Panel
                                        </Link>
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
                            {/* LOGOUT COMPONENT */}
                            <DropdownMenuItem>
                                <LogoutComponent />
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>
                ) : (
                    // User not logged in - show login/register buttons
                    <>
                        <Button
                            asChild
                            variant="outline"
                            size="sm"
                            className="hidden border-slate-300 bg-white/80 text-slate-700 hover:bg-white sm:inline-flex"
                        >
                            <Link href="/auth/login">Đăng nhập</Link>
                        </Button>
                        <Button
                            asChild
                            size="sm"
                            className="hidden bg-primary text-white hover:bg-primary/90 sm:inline-flex"
                        >
                            <Link href="/auth/register">Đăng ký</Link>
                        </Button>
                    </>
                )}
            </div>
        </header>
    );
}
