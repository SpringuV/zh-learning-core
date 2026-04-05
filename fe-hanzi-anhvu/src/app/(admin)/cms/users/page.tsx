import UserManagementComponent from "@/modules/users/components/user.management";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Quản lý người dùng",
    description: "Quản lý danh sách người dùng trong hệ thống",
};

const UserManagementPage = () => {
    return <UserManagementComponent />;
};

export default UserManagementPage;
