namespace Maliev.ShadcnBlazor.Showcase.Theming.Runway;

public sealed class ThemeUseCaseRegistry : IThemeUseCaseRegistry
{
    public IReadOnlyList<ThemeUseCaseDefinition> All { get; } =
    [
        Use("production-capacity", 1, ThemeRunwayTrack.Left, "Production capacity", "กำลังการผลิต", "ShadcnProgress", "ShadcnBadge", "ShadcnButton"),
        Use("operator-profile", 2, ThemeRunwayTrack.Right, "Operator profile", "โปรไฟล์ผู้ปฏิบัติงาน", "ShadcnAvatar", "ShadcnInput", "ShadcnButton"),
        Use("quotation-files", 3, ThemeRunwayTrack.Left, "Quotation files", "ไฟล์ใบเสนอราคา", "ShadcnDropzone", "ShadcnProgress", "ShadcnBadge"),
        Use("shipping-handoff", 4, ThemeRunwayTrack.Right, "Shipping handoff", "การส่งมอบงาน", "ShadcnInput", "ShadcnSelect", "ShadcnCheckbox"),
        Use("inspection-alerts", 5, ThemeRunwayTrack.Left, "Inspection alerts", "การแจ้งเตือนตรวจสอบ", "ShadcnCheckbox", "ShadcnSwitch", "ShadcnButton"),
        Use("deposit-approval", 6, ThemeRunwayTrack.Right, "Deposit approval", "อนุมัติเงินมัดจำ", "ShadcnBadge", "ShadcnSeparator", "ShadcnButton"),
        Use("assigned-reviewers", 7, ThemeRunwayTrack.Left, "Assigned reviewers", "ผู้ตรวจสอบที่ได้รับมอบหมาย", "ShadcnAvatarGroup", "ShadcnBadge", "ShadcnButton"),
        Use("work-order-navigation", 8, ThemeRunwayTrack.Right, "Work-order navigation", "การนำทางใบสั่งงาน", "ShadcnBreadcrumb", "ShadcnItem", "ShadcnBadge"),
        Use("machine-cell", 9, ThemeRunwayTrack.Left, "Machine cell", "สถานีเครื่องจักร", "ShadcnCard", "ShadcnProgress", "ShadcnBadge"),
        Use("assistant-conversation", 10, ThemeRunwayTrack.Right, "Assistant conversation", "บทสนทนาผู้ช่วย", "ShadcnMessageScroller", "ShadcnAvatar", "ShadcnButton"),
        Use("issue-report", 11, ThemeRunwayTrack.Left, "Issue report", "รายงานปัญหา", "ShadcnInput", "ShadcnTextarea", "ShadcnAlert"),
        Use("dispatch-confirmation", 12, ThemeRunwayTrack.Right, "Dispatch confirmation", "ยืนยันการจัดส่ง", "ShadcnAlertDialog", "ShadcnBadge", "ShadcnButton")
    ];

    private static ThemeUseCaseDefinition Use(string id, int order, ThemeRunwayTrack track, string en, string th, params string[] components) =>
        new(id, order, track, en, th, Array.AsReadOnly(components));
}

