using Maliev.ShadcnBlazor.Components.Conversation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class ConversationWorkflowExamples
{
    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch
    {
        "attachment" => [Attachment()], "bubble" => [Bubble()], "marker" => [Marker()], "message" => [Message()],
        "message-scroller" => [Scroller()], "questionnaire" => [Questionnaire()], _ => []
    };

    private static ComponentExampleDefinition Attachment()
    {
        var state = ShadcnAttachmentState.Uploading; var vertical = false; var image = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnAttachment>(0); b.AddAttribute(1,"State",state); b.AddAttribute(2,"Progress",state==ShadcnAttachmentState.Uploading?(double?)64d:null); b.AddAttribute(3,"ErrorReason",state==ShadcnAttachmentState.Error?"อัปโหลดไม่สำเร็จ":null); b.AddAttribute(4,"Orientation",vertical?ShadcnAttachmentOrientation.Vertical:ShadcnAttachmentOrientation.Horizontal); b.AddAttribute(5,"Title","แบบชิ้นงาน.step"); b.AddAttribute(6,"ChildContent",(RenderFragment)(x=>{x.OpenComponent<ShadcnAttachmentMedia>(0);x.AddAttribute(1,"Variant",image?ShadcnAttachmentMediaVariant.Image:ShadcnAttachmentMediaVariant.Icon);x.AddAttribute(2,"ImageAlt",image?"Drawing preview":null);x.AddAttribute(3,"ChildContent",Text(image?"🖼️":"📄"));x.CloseComponent();x.OpenComponent<ShadcnAttachmentContent>(4);x.AddAttribute(5,"ChildContent",(RenderFragment)(c=>{AddText<ShadcnAttachmentTitle>(c,0,"แบบชิ้นงาน.step");AddText<ShadcnAttachmentDescription>(c,3,state==ShadcnAttachmentState.Error?"อัปโหลดไม่สำเร็จ":$"STEP · {state}");}));x.CloseComponent();x.OpenComponent<ShadcnAttachmentActions>(6);x.AddAttribute(7,"ChildContent",(RenderFragment)(a=>{a.OpenComponent<ShadcnAttachmentAction>(0);a.AddAttribute(1,"Action",ShadcnAttachmentActionKind.Remove);a.AddAttribute(2,"AccessibleName","Remove drawing");a.AddAttribute(3,"ChildContent",Text("×"));a.CloseComponent();}));x.CloseComponent();}));b.CloseComponent();};
        return Example("attachment","Attachment lifecycle",preview,[Select("attachment-state","State","Uploading",["Idle","Uploading","Processing","Error","Done"],v=>state=Enum.Parse<ShadcnAttachmentState>(v)),Toggle("attachment-vertical","Vertical",v=>vertical=v),Toggle("attachment-image","Image",v=>image=v)],["idle","uploading","processing","error","done","progress","remove","retry","image","group","rtl"]);
    }

    private static ComponentExampleDefinition Bubble()
    {
        var variant=ShadcnBubbleVariant.Secondary;var end=false;var top=false;
        RenderFragment preview=b=>{b.OpenComponent<ShadcnBubble>(0);b.AddAttribute(1,"Variant",variant);b.AddAttribute(2,"Align",end?ShadcnLogicalAlign.End:ShadcnLogicalAlign.Start);b.AddAttribute(3,"ChildContent",(RenderFragment)(x=>{AddText<ShadcnBubbleContent>(x,0,"ตรวจสอบแบบแล้ว");x.OpenComponent<ShadcnBubbleReactions>(3);x.AddAttribute(4,"Side",top?ShadcnReactionSide.Top:ShadcnReactionSide.Bottom);x.AddAttribute(5,"AccessibleName","Reactions: thumbs up");x.AddAttribute(6,"ChildContent",Text("👍"));x.CloseComponent();}));b.CloseComponent();};
        return Example("bubble","Conversation bubble",preview,[Select("bubble-variant","Variant","Secondary",Enum.GetNames<ShadcnBubbleVariant>(),v=>variant=Enum.Parse<ShadcnBubbleVariant>(v)),Toggle("bubble-end","Align end",v=>end=v),Toggle("bubble-reactions-top","Reactions top",v=>top=v)],["variants","alignment","reactions","button","link","collapsible","rtl"]);
    }

    private static ComponentExampleDefinition Marker()
    {
        var variant=ShadcnMarkerVariant.Default;var streaming=false;
        RenderFragment preview=b=>{b.OpenComponent<ShadcnMarker>(0);b.AddAttribute(1,"Variant",variant);b.AddAttribute(2,"Live",streaming);b.AddAttribute(3,"ChildContent",(RenderFragment)(x=>{AddText<ShadcnMarkerIcon>(x,0,"✓");x.OpenComponent<ShadcnMarkerContent>(3);x.AddAttribute(4,"Streaming",streaming);x.AddAttribute(5,"ChildContent",Text(streaming?"กำลังประมวลผล":"ตรวจสอบ 4 ไฟล์แล้ว"));x.CloseComponent();}));b.CloseComponent();};
        return Example("marker","Conversation marker",preview,[Select("marker-variant","Variant","Default",Enum.GetNames<ShadcnMarkerVariant>(),v=>variant=Enum.Parse<ShadcnMarkerVariant>(v)),Toggle("marker-streaming","Streaming status",v=>streaming=v)],["default","separator","border","status","shimmer","reduced-motion"]);
    }

    private static ComponentExampleDefinition Message()
    {
        var end=false;var avatar=true;var footer=true;
        RenderFragment preview=b=>{b.OpenComponent<ShadcnMessage>(0);b.AddAttribute(1,"Align",end?ShadcnLogicalAlign.End:ShadcnLogicalAlign.Start);b.AddAttribute(2,"ChildContent",(RenderFragment)(x=>{if(avatar)AddText<ShadcnMessageAvatar>(x,0,"ม");x.OpenComponent<ShadcnMessageContent>(3);x.AddAttribute(4,"ChildContent",(RenderFragment)(c=>{AddText<ShadcnMessageHeader>(c,0,"วิศวกร MALIEV");c.OpenComponent<ShadcnBubble>(3);c.AddAttribute(4,"ChildContent",(RenderFragment)(q=>AddText<ShadcnBubbleContent>(q,0,"พร้อมตรวจสอบใบเสนอราคา")));c.CloseComponent();if(footer)AddText<ShadcnMessageFooter>(c,6,"ส่งแล้ว");}));x.CloseComponent();}));b.CloseComponent();};
        return Example("message","Message row",preview,[Toggle("message-end","Align end",v=>end=v),Toggle("message-avatar","Avatar",v=>avatar=v,true),Toggle("message-footer","Footer",v=>footer=v,true)],["start","end","avatar","header","footer","grouped","streaming","rtl"]);
    }

    private static ComponentExampleDefinition Scroller()
    {
        var auto=false;var extra=false;var position=ShadcnMessageDefaultScrollPosition.End;
        RenderFragment preview=b=>{b.OpenComponent<ShadcnMessageScrollerProvider>(0);b.SetKey($"{auto}-{extra}-{position}");b.AddAttribute(1,"AutoScroll",auto);b.AddAttribute(2,"DefaultScrollPosition",position);b.AddAttribute(3,"ChildContent",(RenderFragment)(p=>{p.OpenComponent<ShadcnMessageScroller>(0);p.AddAttribute(1,"Style","height:16rem");p.AddAttribute(2,"data-preview-auto",auto?"true":"false");p.AddAttribute(3,"data-preview-position",position.ToString().ToLowerInvariant());p.AddAttribute(4,"ChildContent",(RenderFragment)(r=>{r.OpenComponent<ShadcnMessageScrollerViewport>(0);r.AddAttribute(1,"AccessibleName","บทสนทนา");r.AddAttribute(2,"ChildContent",(RenderFragment)(v=>{v.OpenComponent<ShadcnMessageScrollerContent>(0);v.AddAttribute(1,"ChildContent",(RenderFragment)(c=>{AddScrollerItem(c,0,"turn-1","ข้อความแรก",true);if(extra)AddScrollerItem(c,5,"turn-2","ข้อความใหม่",true);}));v.CloseComponent();}));r.CloseComponent();r.OpenComponent<ShadcnMessageScrollerButton>(5);r.AddAttribute(6,"AccessibleName","ไปข้อความล่าสุด");r.CloseComponent();}));p.CloseComponent();}));b.CloseComponent();};
        return Example("message-scroller","Streaming transcript",preview,[Toggle("scroller-auto","Auto follow",v=>auto=v),Toggle("scroller-append","Append unread turn",v=>extra=v),Select("scroller-position","Opening position","End",Enum.GetNames<ShadcnMessageDefaultScrollPosition>(),v=>position=Enum.Parse<ShadcnMessageDefaultScrollPosition>(v))],["anchor","auto-follow","user-intent","unread","jump","prepend","visibility","focus","rtl"]);
    }

    private static ComponentExampleDefinition Questionnaire()
    {
        var branch=true;var start="scope";
        RenderFragment preview=b=>{var items=branch?new[]{new ShadcnQuestionnaireItemDefinition("scope",Required:true,Choices:[new("component"),new("feature")]),new("notes",AllowsFreeform:true)}:[new ShadcnQuestionnaireItemDefinition("scope",Required:true,Choices:[new("component"),new("feature")])];b.OpenComponent<ShadcnQuestionnaire>(0);b.SetKey($"{branch}-{start}");b.AddAttribute(1,"Items",items);b.AddAttribute(2,"DefaultItem",start=="notes"&&branch?"notes":"scope");b.AddAttribute(3,"AccessibleName","ขอบเขตงาน");b.AddAttribute(4,"ChildContent",(RenderFragment)(x=>{x.OpenComponent<ShadcnQuestionnaireProgress>(0);x.AddAttribute(1,"AccessibleName","Progress");x.CloseComponent();AddQuestion(x,3,"scope","เลือกขอบเขต",false);if(branch)AddQuestion(x,10,"notes","รายละเอียด",true);x.OpenComponent<ShadcnQuestionnaireActions>(20);x.AddAttribute(21,"ChildContent",(RenderFragment)(a=>{AddText<ShadcnQuestionnairePrevious>(a,0,"ก่อนหน้า");AddText<ShadcnQuestionnaireSkip>(a,3,"ข้าม");AddText<ShadcnQuestionnaireNext>(a,6,"ถัดไป");AddText<ShadcnQuestionnaireSubmit>(a,9,"ส่ง");}));x.CloseComponent();}));b.CloseComponent();};
        return Example("questionnaire","Guided questionnaire",preview,[Toggle("questionnaire-branch","Conditional notes",v=>branch=v,true),Select("questionnaire-start","Resume item","scope",["scope","notes"],v=>start=v)],["single","multiple","freeform","skipped","required","invalid","controlled","resume","branching","submit","thai","rtl"]);
    }

    private static void AddQuestion(RenderTreeBuilder b,int s,string name,string title,bool input){b.OpenComponent<ShadcnQuestionnaireItem>(s);b.AddAttribute(s+1,"Name",name);b.AddAttribute(s+2,"ChildContent",(RenderFragment)(x=>{AddText<ShadcnQuestionnaireTitle>(x,0,title);if(input){x.OpenComponent<ShadcnQuestionnaireInput>(3);x.AddAttribute(4,"AccessibleName",title);x.CloseComponent();}else{x.OpenComponent<ShadcnQuestionnaireChoices>(3);x.AddAttribute(4,"ChildContent",(RenderFragment)(c=>{c.OpenComponent<ShadcnQuestionnaireChoice>(0);c.AddAttribute(1,"Value","component");c.AddAttribute(2,"ChildContent",Text("Component"));c.CloseComponent();c.OpenComponent<ShadcnQuestionnaireChoice>(3);c.AddAttribute(4,"Value","feature");c.AddAttribute(5,"ChildContent",Text("Feature"));c.CloseComponent();}));x.CloseComponent();}x.OpenComponent<ShadcnQuestionnaireError>(8);x.CloseComponent();}));b.CloseComponent();}
    private static void AddScrollerItem(RenderTreeBuilder b,int s,string id,string text,bool anchor){b.OpenComponent<ShadcnMessageScrollerItem>(s);b.AddAttribute(s+1,"MessageId",id);b.AddAttribute(s+2,"ScrollAnchor",anchor);b.AddAttribute(s+3,"ChildContent",Text(text));b.CloseComponent();}
    private static ComponentExampleDefinition Example(string slug,string title,RenderFragment preview,IReadOnlyList<ComponentParameterControl> controls,IReadOnlyList<string> tags)=>new($"{slug}-primary",title,"Live package component with caller-owned localized state.",$"<Shadcn{string.Concat(slug.Split('-').Select(w=>char.ToUpperInvariant(w[0])+w[1..]))} />",preview,controls,tags);
    private static ComponentParameterControl Toggle(string id,string label,Action<bool> apply,bool initial=false)=>new(id,label,ComponentParameterControlKind.Toggle,initial.ToString(),[],v=>apply(bool.Parse(v)));
    private static ComponentParameterControl Select(string id,string label,string initial,IReadOnlyList<string> options,Action<string> apply)=>new(id,label,ComponentParameterControlKind.Select,initial,options,apply);
    private static RenderFragment Text(string value)=>b=>b.AddContent(0,value);
    private static void AddText<T>(RenderTreeBuilder b,int s,string text) where T:IComponent{b.OpenComponent<T>(s);b.AddAttribute(s+1,"ChildContent",Text(text));b.CloseComponent();}
}
