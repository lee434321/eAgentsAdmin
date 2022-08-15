<%@ Page Title="" Language="C#" MasterPageFile="~/SitePublic.Master" AutoEventWireup="true"
    CodeBehind="privacy-policy.aspx.cs" Inherits="PropertyOneAppWeb.Public.privacy_policy" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .disclaimer-td-head
        {
            text-align: center;
            font-weight: bold;
            padding: 0px 0px 20px 0px;
        }
        
        .disclaimer-td-title
        {
            padding: 30px 0px 20px 0px;
            font-weight: bold;
        }
        
        .disclaimer-td-content
        {
        }
        
        .disclaimer-td-sub
        {
            padding: 0px 0px 0px 0px;
        }
        
        .disclaimer-td-sub2
        {
            padding: 0px 0px 0px 40px;
        }
    </style>
    <script type="text/javascript">
        $(document).ready(function () {
            /* 导航栏图片 */
            changeNav("/Image/icon_MyProfile.png", "<%= Resources.Lang.Res_Privacy_Policy%>");
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table style="width: 100%;">
        <tr>
            <td class="disclaimer-td-head">
                Personal Data Statement to Customers (“Statement”)<br />
                HWPL Hong Kong Holdings Limited (“Company”)
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                This Statement is to inform customers of the policies and practices with respect
                to the collection, use, retention, disclosure, transfer, security and access of
                personal data adopted by the Company and its subsidiaries in accordance with the
                Personal Data (Privacy) Ordinance Cap.486 ("Ordinance"). References to “we”, “us”,
                “our” or “ours” in this Statement are to be construed, depending on the context,
                as referring to those entities collectively or severally.
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                1. From time to time, it is necessary for customers to supply us with data in connection
                with the provision of goods and/or services.
                <br />
                <br />
                2. Failure to supply such data may result in us not being able to provide the goods
                and/or services.<br />
                <br />
                3. The customer agrees that any personal data with respect to the customer or any
                other party which has been provided by the customer to us ("Personal Data") may
                be disclosed and transferred to the following persons who shall hold, use, process,
                retain or transfer such Personal Data for the purposes mentioned in paragraph 4:-
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub2">
                (1) any agent, contractor, credit provider, financial institution, service provider
                or professional advisers of ours or any other person under a duty of confidentiality
                to us;
                <br />
                (2) any of our holding companies, their subsidiaries and any company in which any
                of them has a direct interest; and
                <br />
                (3) any actual or proposed assignee or transferee of ours.<br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                4. The customer acknowledges and agrees that his/her Personal Data is provided to
                and retained by us for the following purposes and for other purposes as shall be
                agreed between the customer and us or required by law from time to time:
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub2">
                (1) the sale, leasing or licensing and/or management of property and/or the provision
                of any other related services to the customer (including emergency communications);
                <br />
                (2) the analysis , verification and/or checking of the customer’s credit, credit
                history and payment;
                <br />
                (3) the ensuring of the customer’s on-going credit worthiness;
                <br />
                (4) the determination of the amount of debt owed to or by the customer and/or the
                processing and/or collection of amounts outstanding from the customer and those
                providing security for the customer’s obligations;
                <br />
                (5) the preparation and enforcement of any agreement between the customer and us;
                <br />
                (6) the meeting of the requirements to make disclosures under the requirements of
                any law; and
                <br />
                (7) any other purposes directly relating to the above.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                Where necessary, we will contact the customer for verifying the accuracy of the
                Personal Data collected.<br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                5. We intend to use the customers’ personal information (including their names and
                contact details) for use in direct marketing of the news, offers, services, promotions,
                events and other information in relation to our property development, leasing and
                management businesses. We may not so use the customers’ personal information without
                the customers’ consent or indication of no objection. We will always ask for the
                customers’ consent or indication of no objection before using the customers’ personal
                information for direct marketing. Customers may always opt-out from receiving our
                direct marketing communications by following the instructions in the relevant communications.<br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                6. Under and in accordance with the Ordinance, the customer has the right to:
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub2">
                (1) check whether we hold any Personal Data with respect to the customer;
                <br />
                (2) access such of the Personal Data which we hold with respect to the customer;
                <br />
                (3) require us to correct any Personal Data which is inaccurate; and
                <br />
                (4) ascertain our policies and practices (from time to time) in relation to the
                Personal Data and to be informed of the kinds of the Personal Data held by us.
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                7. In accordance with the Ordinance, we have the right to charge a reasonable fee
                for the processing of any data access request. All request for access to the Personal
                Data or correction of the Personal Data or information regarding policies and practices
                and kinds of Personal Data held should be in writing and addressed to:
                <br />
                Hutchison Property Group Limited
                <br />
                3rd Floor, One Harbourfront
                <br />
                18 Tak Fung Street, Hunghom
                <br />
                Kowloon, Hong Kong
                <br />
                Attn: Personal Data Administrator – Development & Marketing Department (PR & Promotions)
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                8. If there is any inconsistency between the English and Chinese versions of this
                Statement, the English version shall prevail.
            </td>
        </tr>
        <tr style="height: 50px;">
        </tr>
        <tr>
            <td class="disclaimer-td-head">
                HWPL Hong Kong Holdings Limited (下稱"本公司")
                <br />
                致客戶有關個人資料的聲明(下稱"本聲明")
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                本聲明為客戶列出有關本公司及其附屬公司根據《 個人資料（ 私隱） 條例》第486章（下稱「私隱條例」）的規定下收集、使用、保留、公開、轉讓、保障及查閱個人資料的政策及措施。本公司或其附屬公司下稱“我們”。
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                1. 客戶必須不時向我們提供資料，以便向客戶供應產品及／或服務。
                <br />
                <br />
                2. 如客戶未能提供該些資料，可能導致我們未能供應相關的產品及／或服務。<br />
                <br />
                3. 客戶同意凡由客戶向我們提供有關任何客戶或其他人之個人資料（下稱「個人資料」）將可能被披露予或轉讓至以下的人士根據第四段引述之內容以作持有、使用、處理、保留或轉讓該些個人資料的用途：
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub2">
                (1) 我們之任何代理人、承辦商、信貸人、財務機構、服務供應商或專業顧問或任何向我們負有保密責任之其他人士；
                <br />
                (2) 我們任何控股公司、其附屬公司或任何該些公司擁有直接權益的公司；及
                <br />
                (3) 我們之任何實際或提議承讓人或受讓人。<br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                4. 客戶確認及同意客戶向我們提供、並由我們保留之個人資料將用作下列的用途，以及不時由客戶與我們同意或法律規定之其他用途：
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub2">
                (1) 向客戶提供銷售、租賃或借用特許權及／或物業管理及／或向客戶提供任何相關服務(包括緊急聯絡)；
                <br />
                (2) 分析、核查及／或檢查客戶之信貸狀況、信貸記錄及繳款；
                <br />
                (3) 確保客戶是持續有可靠的信貸償還能力；
                <br />
                (4) 計算客戶應收或應付欠款的數目，及／或處理客戶的欠款及／或向客戶徵收欠款；
                <br />
                (5) 客戶與我們之間的任何協議的擬訂和執行；
                <br />
                (6) 按任何法律規定進行之有關披露；及
                <br />
                (7) 任何與以上事項直接有關之用途。
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                如有需要，我們會聯絡客戶核對已收集的個人資料的準確性。
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                5. 我們擬使用客戶個人資料（包括客戶姓名及聯絡資料）作有關我們的物業發展、租賃及管理業務的新聞發佈、優惠宣傳、服務推廣、產品推廣及其他相關信息之直接促銷。沒有取得客戶同意或不反對前，我們不會使用該客戶的個人資料。我們需徵求並取得客戶的同意或不反對後方可使用該客戶的個人資料用作直接促銷。如客戶不希望我們將其個人資料用作宣傳或推廣產品或服務之用途，可隨時取消訂閱。
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                6. 根據及依照私隱條例，客戶有權：
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub2">
                (1) 確查我們是否持有其任何個人資料；
                <br />
                (2) 查閱該些我們持有有關其本人之個人資料；
                <br />
                (3) 要求我們更正任何錯誤之個人資料；及
                <br />
                (4) 不時查閱我們有關個人資料的政策及措施，以及得知我們所持有之個人資料類別。
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                7. 根據私隱條例，我們有權就處理任何查閱資料要求收取合理費用。所有申請查閱或更改有關之個人資料或索取個人資料政策及措施及類別之資料，須以書面形式提出，並郵寄至以下地址：
                <br />
                九龍紅磡
                <br />
                德豐街18號
                <br />
                海濱廣場1座3樓
                <br />
                和記地產集團有限公司
                <br />
                致：市場及拓展部(公共關係及推廣) – 個人資料管理員
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                8. 本聲明為英文版本譯本，如此版本與英文版本有任何抵觸或不相符之處，概以英文版本為準。
            </td>
        </tr>
    </table>
</asp:Content>
