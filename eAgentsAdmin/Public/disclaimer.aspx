<%@ Page Title="" Language="C#" MasterPageFile="~/SitePublic.Master" AutoEventWireup="true"
    CodeBehind="disclaimer.aspx.cs" Inherits="PropertyOneAppWeb.Public.disclaimer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
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
            changeNav("/Image/icon_MyProfile.png", "<%= Resources.Lang.Res_Login_Button_Term%>");
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table style="width: 100%;">
        <tr>
            <td class="disclaimer-td-title">
                1. ACCEPTANCE OF TERMS AND CONDITIONS
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                Hutchison Estate Agents Limited (“HEAL”) operates and administers a website at www.heal.com.hk
                (“Site”) which provides users of the Site with access to e-statement services ("Services")
                relating to their tenancies of the premises owned by landlord companies within the
                group of HWPL Hong Kong Holdings Limited (“Landlords”) for whom HEAL is acting as
                agents. The terms "HEAL" and "we" as used herein refer to HEAL and the Landlords
                and unless the context otherwise requires such expression shall also include any
                of its directors, employees, officers and agents (and the terms “us” and “our” shall
                be so construed accordingly).
                <br />
                <br />
                The terms "you", "User" and “Users” as used herein refer to all individuals and
                / or entities accessing the Services and/or the Site. The term "Contents" is used
                herein to refer to all or any of, as the case may be, the data, text, button icons,
                links, HTML codes, trademark, software, music, sound, photographs, graphics, still
                pictures, series of moving pictures (whether animated or not), videos, merchandise,
                products, advertisements, services or any compilation or combination of them and
                any other contents or materials that may be found on the Site.
                <br />
                <br />
                HEAL provides the Services to you, subject to the following Terms and Conditions
                of Use ("T&C"), which may be updated and revised by us from time to time by posting
                the revised version on the Site. Unless otherwise stated by us, any changes we make
                will be effective immediately upon posting. Your use of the Site after such posting
                amounts to your conclusive acceptance of such change. Be sure to review the T&C
                regularly to ensure familiarity with the most current version. You undertake that
                you will not assert your lack of knowledge or our lack of notification of any changes
                to the relevant terms and conditions as a defence in the event of a dispute. Some
                of the Services may be subject to additional terms and conditions governing their
                provision which additional terms will be made known to you upon you expressing your
                intent to use those Services. Those additional terms and conditions together with
                the Privacy Policy (as referred to in paragraph 4 below) are hereby incorporated
                by reference into the T&C.
                <br />
                <br />
                You accept and agree to be bound by the T&C upon your registration for/using the
                Services or otherwise accessing the Site or using any information found therein.
                If you do not accept the T&C, you may not and should not access the Site or use
                the Services or information provided thereunder. Any other terms and conditions
                proposed by you which are in addition to or which conflict with the T&C are expressly
                rejected by HEAL and shall be of no force and effect. If you have any questions
                about the T&C, or about accessing and using the Site, please contact HEAL. We reserve
                the right to interpret the T&C and decide on any questions or disputes arising under
                the T&C. You agree that all such interpretations and decisions shall be final and
                conclusive, and binding on you as a user of the Site and/or the Services.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                2. REGISTRATION, USER ACCOUNT AND PASSWORD
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                You will be asked to complete registration on the Site before the Services become
                available to you. In completing the registration, you undertake to (a) provide true,
                accurate, current and complete information as required and (b) maintain and promptly
                update such information to keep it true, accurate, current and complete. If you
                provide any information that is untrue, inaccurate, not current or incomplete, or
                HEAL has reasonable grounds to suspect that such is untrue, inaccurate, not current
                or incomplete, HEAL has the right to suspend or terminate your account and refuse
                any and all current or future use of the Services (or any portion thereof).
                <br />
                <br />
                Upon successful registration by a User, an account ("User Account") will be set
                up for the User, who will be provided with a password ("Password") to enable the
                User to access and use of the Site and/or the Service. You are responsible for maintaining
                the confidentiality of your User Account and/or Password (as may be altered from
                time to time). You are fully responsible (and shall be liable to HEAL) for all activities
                that occur under the User Account and/or Password.
                <br />
                <br />
                Users shall give a prior notification, according to the specific requirements given
                by HEAL from time to time, informing HEAL to terminate or update any changes on
                the Services for the Users’ respective User Account(s).
                <br />
                <br />
                You are strictly prohibited from assigning, transferring, licensing or sub-licensing
                to any other person their right to access or use the Site and/or the Service or
                any part thereof. You shall not use, or allow anyone to use, any means to circumvent
                log in password and other protections which HEAL may put in place to restrict access
                to certain areas of the Site and/or the Service.
                <br />
                <br />
                You shall immediately notify HEAL of any unauthorised use of any User Account or
                Password or any other breach of security through any User Account.
                <br />
                <br />
                You acknowledge and agree that the only duty of HEAL is to verify Passwords inputted
                by the Users and HEAL shall not be liable in respect of:
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                (a) any loss or damage suffered by you or any other person as a result of any failure
                to effect or execute instructions through various electronic delivery channels or
                perform any obligation thereunder where such failure is attributable either directly
                or indirectly to any circumstances or events outside our control; or
                <br />
                (b) any other loss or damage whatsoever suffered by you or by any other person as
                a result of any instructions through various electronic delivery channels given
                with the correct Password and/or other information.
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                You note that only Secure Sockets Layer (SSL) Software, which encrypts information
                you input, is used to protect the security of your personally identifiable information
                during transmission. By using or accessing the Site and/or the Services and in consideration
                of such access and use, you acknowledge that you are satisfied that the security
                features that we have adopted are adequate for all your use of the Site and/or the
                Services in accordance with the T&C.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                3. SERVICES
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                All Services together with any new features that augment or enhance any such services
                currently offered shall, unless explicitly stated otherwise, be subject to the T&C.
                <br />
                <br />
                You agree, understand and acknowledge that:
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                (a) HEAL will send notifications by e-mail to the Users’ designated e-mail addresses
                informing the Users that the e-statements of their respective registered User Account
                in electronic form is available for viewing online and the Users shall check their
                designated e-mail addresses regularly for such notifications;
                <br />
                (b) HEAL will retain the e-statements of the registered User Account(s) at the Site
                for a period of 6 months (or such other period as prescribed by HEAL from time to
                time) and the Users shall examine each e-statement upon receiving the e-mail notice
                from HEAL and if necessary, print and/or download the e-statement for future reference;
                <br />
                (c) HEAL is entitled to levy fee and charges against the Users to cover the cost
                and expenses for the Users requisition of obtaining a hard copy of e-statement that
                is no longer available for access and downloading through the Site and in any event,
                HEAL is not under any obligation to comply with any such request; and
                <br />
                (d) User Accounts (and access to the Services via the same) will be disabled/ terminated
                upon the expiry or earlier termination of the Users’ respective tenancies with the
                Landlords.
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                You acknowledge that there are inherent hazards in communications over the Internet
                and telecommunication networks and as such there may be delay, omission or inaccuracies
                in the information so provided. You also understand and agree that the Services
                are provided on an "AS IS" and "AS AVAILABLE" basis and that HEAL assumes no responsibility
                for the timeliness, deletion, mis-delivery or failure of the provision of any of
                the Services.
                <br />
                <br />
                You understand that the technical processing and transmission of the Services may
                involve (a) transmissions over various networks; and (b) changes to conform and
                adapt to technical requirements of connecting networks, devices or media. HEAL shall,
                accordingly, in no circumstances, be liable for any failure of any Services in whole
                or in part or for your inability to gain access in whole or in part to the Site
                and/or the Services due to the delay or failure of any communication networks, devices
                or media or any party providing such access or necessary support including power
                supply. We do not guarantee uninterrupted, continuous or secure access to the Site.
                Part or the entire Site and/or the Services may be unexpectedly unavailable for
                whatever duration and for various reasons that may include system malfunctions and
                disruptions, Internet and/or telecommunication network access downtime and other
                technical problems beyond our control for which we cannot and shall not be held
                responsible. You agree that you will not hold us responsible for any damage or loss
                caused by your inability to use the Site and/or the Services for any reason whatsoever.
                We reserve the right to take any part or all of the Site and/or the Services offline
                for various reasons including urgent system maintenance or upgrading, in which case
                we will try to give you notice in advance as far as practically possible.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                4. PRIVACY POLICY
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                Personally identifiable information is subject to our Privacy Policy. Please see
                our Privacy Policy <a href="/Public/privacy-policy.aspx" target="_blank">here</a>.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                5. USER CONDUCT
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                You undertake not to:
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                (a) interfere with, impair or disrupt the provision or operation of the Services
                or servers or networks or telecommunication service through which the Services are
                provided, or disobey any requirements, procedures, policies or regulations of such
                networks or telecommunication service;
                <br />
                (b) attempt to gain unauthorised access to the Site, other User's accounts or passwords,
                computer systems or networks connected to the mobile app, through password mining
                or any other means;
                <br />
                (c) modify, adapt, sub-license, translate, sell, reverse engineer, decompile or
                disassemble any portion of the Site or software contained in the Siteor used in
                connection with the Services (including any files, images incorporated in or generated
                by the software, and data accompanying the software);
                <br />
                (d) remove any copyright, trademark, or other proprietary rights notices contained
                in the Site;
                <br />
                (e) "frame" or "mirror" any part of the Site without our prior written authorisation;
                <br />
                (f) use any robot, "spider", site search/retrieval application, or other manual
                or automatic device or process to retrieve, index, "data mine", or in any way reproduce
                or circumvent the navigational structure or presentation of the Siteor its contents;
                or
                <br />
                (g) collect or store personally identifiable information about other Users.
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                Without limiting the generality of the foregoing, you further agree not to trespass,
                break into, access, use or attempt to trespass, break into, access or use any other
                parts of our servers and/or any data areas for which you have not been authorised
                by us.
                <br />
                <br />
                Any unauthorised alteration or addition to the Site and any information, data and
                material contained therein is strictly prohibited. All rights not expressly granted
                herein are reserved. These T&C supersede any prior or contemporaneous communications
                between us (including any of our employees, officers, directors and agents) and
                you in respect of the Site. We reserve the rights from time to time, without notice,
                to access your User Account or to observe and record your access to and use of the
                Site to determine if you are complying with the T&C.
                <br />
                <br />
                You shall comply with all applicable laws, statutes, ordinances and regulations
                (whether or not having the force of law) (the "Applicable Laws ") regarding your
                use of the Site. You recognise the global nature of the Internet and you understand
                that the Applicable Laws may be of a jurisdiction other than your own and you agree
                that compliance with the Applicable Laws is your sole responsibility. We recommend
                that you seek legal advice on your own account if you are not sure what Applicable
                Laws comprise.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                6. INDEMNITY
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                You agree to indemnify, defend and hold harmless HEAL, and its parent/holding companies,
                subsidiaries, affiliates, officers, agents, co-branders or other partners, and employees
                against any and all claims, proceedings, damages, liabilities, cost and expenses
                (including all legal costs on full indemnity basis) arising from:
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                (a) your acts or inaction in breach of these T&C;
                <br />
                (b) your use of the Services or any use of the Site through your User Account;
                <br />
                (c) the Contents or personally identifiable information submitted, posted to or
                transmitted through the Services/ the Site by you or using your User Account; and/or
                <br />
                (d) the provision of the Services by HEAL, whether or not arising from or in connection
                with:
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub2">
                (i) improper use of the Site, the Service, the e-statements by you or any other
                person using your User Account (whether authorized by you or not);
                <br />
                (ii) inaccurate information provided by you in relation to the User Account, e-mail
                address(es), contact person(s), contact number(s) or any other information; or
                <br />
                (iii) any damage to the computer hardware, devices, facilities or software as a
                result of your accessing and/or using the Site and/or Service.
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                Termination of your right to use the Services or the Site by either HEAL or you
                shall in no way removes or otherwise affects your obligation to indemnify HEAL hereunder.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                7. MODIFICATIONS TO SERVICES
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                HEAL reserves the right at any time and from time to time to modify or discontinue,
                temporarily or permanently, the Site and/or the Services (or any part thereof) that
                may be available to you without prior notice.
                <br />
                <br />
                You agree that HEAL shall not be liable to you or to any third party for any modification,
                suspension or discontinuance of the Services and/or the Site.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                8. DISCLAIMER
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                YOU EXPRESSLY UNDERSTAND AND AGREE THAT:
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                A. YOUR USE OF THE SERVICES AND/OR THE SITE IS AT YOUR OWN RISK. TO THE FULLEST
                EXTENT PERMITTED BY LAW, HEAL EXPRESSLY DISCLAIMS ALL WARRANTIES OF ANY KIND, WHETHER
                EXPRESS OR IMPLIED.
                <br />
                B. HEAL MAKES NO WARRANTY THAT: (I) THE SERVICES WILL MEET YOUR REQUIREMENTS; (II)
                THE SERVICES WILL BE UNINTERRUPTED, TIMELY, SECURE, OR ERROR-FREE; (III) THE RESULTS
                THAT MAY BE OBTAINED FROM THE USE OF THE SERVICES WILL BE ACCURATE OR RELIABLE;
                (IV) THE QUALITY OF ANY CONTENTS OR OBTAINED BY YOU THROUGH THE SERVICES WILL MEET
                YOUR EXPECTATIONS; OR (V) ANY ERRORS IN ANY SOFTWARE WILL BE CORRECTED.
                <br />
                C. ANY CONTENTS DOWNLOADED OR OTHERWISE OBTAINED THROUGH THE USE OF THE SERVICES
                AND/OR THE SITE ARE OBTAINED AT YOUR OWN DISCRETION AND RISK AND THAT YOU WILL BE
                SOLELY RESPONSIBLE FOR ANY DAMAGE TO YOUR COMPUTER SYSTEM OR LOSS OF DATA THAT RESULTS
                FROM THE DOWNLOAD OF ANY SUCH CONTENTS.
                <br />
                D. NO ADVICE OR INFORMATION, WHETHER ORAL OR WRITTEN, OBTAINED BY YOU FROM HEAL
                OR THROUGH OR FROM THE SERVICES SHALL CONSTITUTE ANY WARRANTY.
                <br />
                E. HEAL DOES NOT GUARANTEE TIMELINESS OF THE SERVICES AND YOU ALSO AWARE THAT CERTAIN
                SERVICES ARE PROVIDED ON A DELAYED BASIS.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                9. LIMITATION OF LIABILITY
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                YOU UNDERSTAND AND EXPRESSLY AGREE THAT, TO THE FULLEST EXTENT PERMITTED BY LAW,
                HEAL SHALL NOT BE LIABLE WHETHER IN TORT OR CONTRACT OR OTHERWISE FOR ANY DIRECT,
                INDIRECT, INCIDENTAL, SPECIAL, CONSEQUENTIAL OR EXEMPLARY DAMAGES, INCLUDING BUT
                NOT LIMITED TO, DAMAGES FOR LOSS OF PROFITS, GOODWILL, REVENUE, USE, DATA OR OTHER
                INTANGIBLE LOSSES, RESULTING FROM: (I) THE USE, THE INABILITY TO USE OR THE UNAVAILABILITY
                OF THE SERVICES AND/OR THE SITE; (II) UNAUTHORIZED ACCESS TO OR ALTERATION OF YOUR
                USER ACCOUNTS, PASSWORDS, TRANSMISSIONS OR PERSONALLY IDENTIFIABLE INFORMATION;
                OR (III) ANY OTHER MATTER RELATING TO THE SERVICES AND/OR THE SITE.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                10. INTELLECTUAL PROPERTY RIGHTS
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                The Site together with all Contents made available as part of theSite are our property
                or are licensed to us and are protected by copyright, trademarks, service marks,
                patents or other proprietary rights and laws with all rights reserved. We and/or
                our licensors own copyright in the selection, co-ordination, arrangement and enhancement
                of such Contents, as well as in the content original to it. Unauthorized copying,
                distribution, modification, exploitation and public display of or dealings with
                copyrighted works is an infringement of the copyright holders' rights. HEAL and
                other parties own the trademarks, logos and service marks displayed on the Site
                and any necessary software used in connection with the Services and Users are prohibited
                from using, modifying, lending, selling, renting, leasing, distributing or creating
                derivative works based on or in any way tempering with the same, in whole or in
                part, without the written permission of HEAL or such other parties (as the case
                may be). HEAL reserves the right to terminate the registration of any User upon
                notice of any infringement of the copyrights or other intellectual property rights
                of others in conjunction with use of the Site and/or the Servcies.
                <br />
                <br />
                The availability of any information, data and materials contained in the Site in
                all circumstances should not be taken to be or constitute a transfer of copyrights,
                trademarks or other intellectual property rights of HEAL and/or any licensor in
                the Site and the information, data and materials contained therein to any users
                or visitors of the Site or any other third parties.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                11. NO AGENCY
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                You and HEAL are independent and no agency, partnership, joint venture, trustee,
                beneficiary, employee-employer or franchiser-franchisee relationship is intended
                or created by your use of the Site or the Services.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                12. WAIVER
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                No right under the T&C shall be deemed to be waived except by notice in writing
                signed by HEAL and you. A waiver by HEAL will not prejudice its rights in respect
                of any subsequent breach of the T&C by you.
                <br />
                <br />
                Subject to the foregoing provisions of this Clause 12, any failure by HEAL to enforce
                any provisions of the T&C, or any forbearance, delay or indulgence granted by HEAL
                to you, will not be construed as a waiver by HEAL of its rights or remedies under
                the T&C.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                13. NOTICES
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                Notices to HEAL under the T&C may be delivered by e-mail to the addresses shown
                at “Contact Us” in the Site.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                14. GOVERNING LAW
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                The T&C shall be construed in accordance with the laws of the Hong Kong Special
                Administrative Region of the People's Republic of China ("Hong Kong ").
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                15. SEVERABILITY
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                If any one or more of these T&C, or their application in any circumstance, is held
                invalid, illegal or unenforceable in any respect for any reason, the validity, legality
                and enforceability of that term or condition in any other respect and the remaining
                T&C shall not in any way be impaired.
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-title">
                16. GENERAL PROVISIONS
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-content">
                Unless the context otherwise requires, the T&C should be interpreted using the following
                rules:
            </td>
        </tr>
        <tr>
            <td class="disclaimer-td-sub">
                (a) words importing one gender include the other genders;
                <br />
                (b) words importing the singular shall include the plural and vice versa;
                <br />
                (c) references to Clauses and references to clauses of the T&C;
                <br />
                (d) expressions defined in the main body of the T&C bear the defined meanings in
                the whole of the T&C;
                <br />
                (e) headings are for ease of reference only and shall not affect the interpretation
                of the T&C;
                <br />
                (f) any reference to a person shall include that person's successors, representatives
                and permitted assigns; and
                <br />
                (g) in the event that there is any inconsistency between the English and Chinese
                versions of the T&C, the English version shall prevail.
            </td>
        </tr>
    </table>
</asp:Content>
