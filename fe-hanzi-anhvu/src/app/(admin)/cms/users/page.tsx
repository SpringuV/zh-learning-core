"use client";
import { useUserList } from "@/modules/users/hooks/use.user.query";

const UserManagementPage = () => {
    const userManagementData = useUserList();
    console.log("User list data:", userManagementData.data);
    return <div>User Management Page</div>;
};

export default UserManagementPage;
