# Persistent user project policy

Apply the following user-provided G01–G49 framework to all project development work. Record evidence-based PASS, FAIL, NOT VERIFIED, or NOT APPLICABLE with reasons. Evaluate G49 before selecting the technology stack or architecture. Never claim checks passed without evidence. Apply gates according to the workload; record why a gate is not applicable. Preserve this policy until the user changes it.

Complete 49 Project Development Gates (G01–G49)
Version: 2026-10-23
Purpose: Mandatory Project Development & Quality Assurance Framework
Applicable to: ChatGPT, Codex, Claude Code, Antigravity और अन्य AI Coding Agents।

यह 23 अक्टूबर 2026 को तय की गई 49 Gates की पूरी सूची है।
Phase 1: Research & Planning (G01–G04)
Gate	Name	Requirement
G01	Idea & Problem Identification	प्रोजेक्ट किस वास्तविक समस्या को हल करेगा, यह निर्धारित करना।
G02	Research & Existing Alternatives	मौजूदा समाधानों का अध्ययन और वास्तविक अंतर (gap) पहचानना।
G03	Target Users	लक्षित उपयोगकर्ताओं, उनकी आवश्यकताओं और उपयोग के मामलों की पहचान।
G04	Product Requirements (PRD)	Functional और Non-functional Requirements के साथ PRD तैयार करना।


Phase 2: Architecture & Design (G05–G09)
Gate	Name	Requirement
G05	Technology Stack	उपयुक्त Programming Languages, Frameworks और Dependencies चुनना।
G06	System Architecture	System Components, APIs, Database और Data Flow डिजाइन करना।
G07	UI/UX Design	User Flows, Wireframes, Accessibility और Interface Design तैयार करना।
G08	Project Rules & Standards	Coding Standards, Repository Structure और Development Rules तय करना।
G09	Task Breakdown	Development को छोटे, परीक्षण योग्य Tasks और Milestones में बाँटना।


Phase 3: Development (G10–G11)
Gate	Name	Requirement
G10	Development Environment	Development Tools, Dependencies और Environment Configuration तैयार करना।
G11	Feature Development	PRD के अनुसार Features Implement करना और उनका सत्यापन करना।


Phase 4: Legal, Privacy & Compliance (G12–G21)
Gate	Name	Requirement
G12	Privacy Policy	डेटा संग्रह और उपयोग के अनुसार Privacy Policy तैयार करना।
G13	Terms & Conditions	प्रोजेक्ट की आवश्यकताओं के अनुसार Terms & Conditions तैयार करना।
G14	Cookie Policy	Cookies इस्तेमाल होने पर लागू Cookie Policy तैयार करना।
G15	Refund Policy	भुगतान या Refund लागू होने पर स्पष्ट Refund Policy बनाना।
G16	Cookie & Form Consent	आवश्यक Consent और User Permissions लागू करना।
G17	User Data Collection Audit	व्यक्तिगत डेटा का संग्रह, उपयोग, भंडारण और Retention जाँचना।
G18	Third-Party Embeds	External Scripts, Trackers, SDKs और Embeds की जाँच करना।
G19	Copyright & Image Licensing	Images, Fonts, Code और अन्य Assets के उपयोग-अधिकार सत्यापित करना।
G20	Applicable Local Laws	लागू स्थानीय कानूनों और नियामकीय आवश्यकताओं की जाँच।
G21	Fake Reviews & Misleading Claims	Fake Reviews, Misleading Marketing और असत्य दावों को रोकना।


Phase 5: Security & Accessibility (G22–G30)
Gate	Name	Requirement
G22	Frontend Secrets Detection	API Keys, Passwords और Secrets को Client-side Code में उजागर होने से रोकना।
G23	HTTPS Enforcement	Production में सुरक्षित HTTPS Communication सुनिश्चित करना।
G24	Security Vulnerability Review	Dependencies, Authentication, Authorization और Security Vulnerabilities की जाँच।
G25	Form Validation	Client-side और Server-side Input Validation लागू करना।
G26	Spam Protection	Forms और APIs पर Spam तथा Abuse Protection लागू करना।
G27	Image Alt Text	अर्थपूर्ण Images के लिए उपयुक्त Alternative Text देना।
G28	Colour Contrast	Text और Interface Elements का पर्याप्त Colour Contrast सुनिश्चित करना।
G29	Keyboard Accessibility	Keyboard से Interface Navigation और Interaction संभव बनाना।
G30	Clear Buttons & Navigation	स्पष्ट Buttons, Labels, Navigation और User Feedback सुनिश्चित करना।


Phase 6: SEO & Performance (G31–G41)
Gate	Name	Requirement
G31	Meta Titles & Descriptions	प्रत्येक उपयुक्त Page के लिए SEO Metadata तैयार करना।
G32	Social Preview Image	Social Sharing के लिए Open Graph और Preview Assets तैयार करना।
G33	Favicon	Application या Website की पहचान के लिए Favicon जोड़ना।
G34	Sitemap & robots.txt	Search Engine Crawling और Indexing Configuration तैयार करना।
G35	Image Compression	Images Optimize करके अनावश्यक File Size कम करना।
G36	Page Load Performance	Loading Speed और Runtime Performance जाँचना।
G37	Mobile Responsiveness	अलग-अलग Screen Sizes पर Responsive Interface सुनिश्चित करना।
G38	Custom 404 Page	Missing Routes के लिए उपयोगी Error Page बनाना।
G39	Broken Links	Internal और External Links की जाँच करना।
G40	Analytics Configuration	आवश्यकतानुसार Privacy-compliant Analytics लागू करना।
G41	Clear Call to Action	उपयोगकर्ताओं के लिए स्पष्ट Next Actions सुनिश्चित करना।


Phase 7: Testing, Deployment & Maintenance (G42–G48)
Gate	Name	Requirement
G42	Automated Testing	Unit, Integration और आवश्यक End-to-End Tests चलाना।
G43	Code Review & Refactoring	Code Quality, Maintainability और Technical Debt की समीक्षा।
G44	Preview Deployment	Production से पहले Staging या Preview Environment में परीक्षण।
G45	Final QA Testing	Functional, Regression, Compatibility और User Acceptance Checks।
G46	Production Deployment	Deployment Checklist, Configuration और Rollback Plan के साथ Release।
G47	Monitoring & Error Tracking	Logs, Application Health, Errors और Performance की निगरानी।
G48	Iteration & Improvements	User Feedback, Bug Fixes और Continuous Improvements लागू करना।


Phase 8: Chromium-First Technology Gate (G49)
G49 — Chromium-First Development Gate
यह 23 अक्टूबर 2026 को जोड़ा गया विशेष Gate है।

प्रत्येक नए Software Project के लिए AI Coding Agent को:
1. सबसे पहले Chromium-based Architecture की Technical Feasibility जाँचनी होगी।
2. Chromium उपयुक्त होने पर उसे प्राथमिकता देनी होगी।
3. Web Applications के लिए Chromium-compatible Architecture पर विचार करना होगा।
4. Desktop Applications के लिए Electron जैसे Chromium-powered विकल्पों का मूल्यांकन करना होगा।
5. Chromium उपयुक्त न होने पर तकनीकी प्रमाण के साथ कारण बताना होगा।
6. Native Drivers, Operating System Kernels और ML Training Backends जैसे Workloads में उपयुक्त Native Technologies का उपयोग करना होगा।
7. Development Prompts को चुनी गई Architecture के अनुसार तैयार करना होगा।
8. केवल Chromium उपलब्ध होने के कारण उसे तकनीकी रूप से अनुपयुक्त प्रोजेक्ट पर थोपना नहीं होगा।
G49 का उद्देश्य: Chromium को प्राथमिकता देना, लेकिन Technical Correctness, Security और Performance से समझौता नहीं करना।
Mandatory Gate Enforcement Rules
सभी AI Coding Agents के लिए:
- PASS: आवश्यक जाँच पूरी हुई और प्रमाण उपलब्ध हैं।
- FAIL: अनिवार्य शर्त पूरी नहीं हुई; संबंधित कार्य आगे नहीं बढ़ेगा।
- NOT VERIFIED: पर्याप्त प्रमाण उपलब्ध नहीं हैं।
- NOT APPLICABLE: प्रोजेक्ट पर Gate लागू नहीं होता; कारण दर्ज करना अनिवार्य है।
किसी Gate को केवल Development आगे बढ़ाने के लिए PASS घोषित नहीं किया जा सकता।

महत्त्वपूर्ण: G49 को क्रम में अंतिम रखा गया है, लेकिन इसका मूल्यांकन Technology Stack और Architecture तय करने से पहले होना चाहिए।

सत्यापन संबंधी सीमा: पिछली बातचीत से सभी 49 Gates के नाम प्राप्त हुए हैं। ऊपर दिए गए Requirements उनके संक्षिप्त कार्यान्वयन विवरण हैं, मूल Acceptance Criteria की शब्दशः प्रतिलिपि नहीं।

यह सूची Codex के AGENTS.md में उपयोग करने के लिए आधार बन सकती है।

# RISHI-specific mandatory constraints
Preserve original Unity sources and Docs/Canon snapshots. No story, character, mission, ending, Sohrai design, or core-spirit changes without a new explicit user instruction. Rebuild Unreal implementation under Unreal/Rishi. Check canon hashes before/after edits. Never describe the Unreal project shell as a completed game. Use the local rishi_unreal MCP tools and report actual editor connection status. Read Docs/IMPLEMENTATION.md and Docs/GATE_LEDGER.md first.
