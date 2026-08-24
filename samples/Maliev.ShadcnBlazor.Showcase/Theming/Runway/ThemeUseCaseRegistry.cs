namespace Maliev.ShadcnBlazor.Showcase.Theming.Runway;

public sealed class ThemeUseCaseRegistry : IThemeUseCaseRegistry
{
    public IReadOnlyList<ThemeUseCaseDefinition> All { get; } =
    [
        Use("production-capacity", 1, ThemeBentoSize.Standard, "Production capacity", "กำลังการผลิต", "ShadcnProgress", "ShadcnBadge", "ShadcnButton"),
        Use("operator-profile", 2, ThemeBentoSize.Standard, "Operator profile", "โปรไฟล์ผู้ปฏิบัติงาน", "ShadcnAvatar", "ShadcnInput", "ShadcnButton"),
        Use("quotation-files", 3, ThemeBentoSize.Wide, "Quotation files", "ไฟล์ใบเสนอราคา", "ShadcnDropzone", "ShadcnProgress", "ShadcnBadge"),
        Use("shipping-handoff", 4, ThemeBentoSize.Standard, "Shipping handoff", "การส่งมอบงาน", "ShadcnInput", "ShadcnSelect", "ShadcnCheckbox"),
        Use("inspection-alerts", 5, ThemeBentoSize.Standard, "Inspection alerts", "การแจ้งเตือนตรวจสอบ", "ShadcnCheckbox", "ShadcnSwitch", "ShadcnButton"),
        Use("deposit-approval", 6, ThemeBentoSize.Standard, "Deposit approval", "อนุมัติเงินมัดจำ", "ShadcnBadge", "ShadcnSeparator", "ShadcnButton"),
        Use("assigned-reviewers", 7, ThemeBentoSize.Standard, "Assigned reviewers", "ผู้ตรวจสอบที่ได้รับมอบหมาย", "ShadcnAvatarGroup", "ShadcnBadge", "ShadcnButton"),
        Use("work-order-navigation", 8, ThemeBentoSize.Wide, "Work-order navigation", "การนำทางใบสั่งงาน", "ShadcnBreadcrumb", "ShadcnItem", "ShadcnBadge"),
        Use("machine-cell", 9, ThemeBentoSize.Standard, "Machine cell", "สถานีเครื่องจักร", "ShadcnCard", "ShadcnProgress", "ShadcnBadge"),
        Use("assistant-conversation", 10, ThemeBentoSize.Wide, "Assistant conversation", "บทสนทนาผู้ช่วย", "ShadcnMessageScroller", "ShadcnAvatar", "ShadcnButton"),
        Use("issue-report", 11, ThemeBentoSize.Standard, "Issue report", "รายงานปัญหา", "ShadcnInput", "ShadcnTextarea", "ShadcnAlert"),
        Use("dispatch-confirmation", 12, ThemeBentoSize.Standard, "Dispatch confirmation", "ยืนยันการจัดส่ง", "ShadcnAlertDialog", "ShadcnBadge", "ShadcnButton"),
        Use("quotation-actions", 13, ThemeBentoSize.Standard, "Quotation actions", "การดำเนินการใบเสนอราคา", "ShadcnDropdownMenu"),
        Use("reviewer-details", 14, ThemeBentoSize.Standard, "Reviewer details", "รายละเอียดผู้ตรวจสอบ", "ShadcnHoverCard", "ShadcnTooltip", "ShadcnAvatar"),
        Use("file-context", 15, ThemeBentoSize.Standard, "Drawing workspace", "พื้นที่ทำงานแบบ", "ShadcnContextMenu"),
        Use("contact-dialog", 16, ThemeBentoSize.Standard, "Production contact", "ผู้ติดต่อฝ่ายผลิต", "ShadcnDialog", "ShadcnInput"),
        Use("dispatch-drawer", 17, ThemeBentoSize.Standard, "Dispatch review", "ตรวจสอบการจัดส่ง", "ShadcnDrawer"),
        Use("delivery-sheet", 18, ThemeBentoSize.Standard, "Delivery schedule", "กำหนดการส่งมอบ", "ShadcnSheet"),
        Use("tooltip-guidance", 19, ThemeBentoSize.Standard, "Inspection guidance", "คำแนะนำการตรวจสอบ", "ShadcnTooltip", "ShadcnButton"),
        Use("project-questionnaire", 20, ThemeBentoSize.Standard, "Production review questionnaire", "แบบสอบถามการตรวจสอบงาน", "ShadcnQuestionnaire", "ShadcnQuestionnaireChoice", "ShadcnQuestionnaireInput")
    ];

    private static ThemeUseCaseDefinition Use(string id, int order, ThemeBentoSize size, string en, string th, params string[] components) =>
        new(id, order, size, en, th, Array.AsReadOnly(components));
}

