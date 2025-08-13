using Freelancing.Data;
using Freelancing.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Freelancing
{
    public class SeedUserSkills
    {
        public static async Task SeedUserSkillsData(ApplicationDbContext context)
        {
            // Check if UserSkills already exist
            if (await context.UserSkills.AnyAsync())
            {
                return; // UserSkills already seeded
            }

            var userSkills = new List<UserSkill>();

            // Websites, IT & Software Category
            var websitesItSoftwareSkills = new[]
            {
                ".NET", ".NET 5.0/6", ".NET Core", ".NET Core Web API", "4D", "A-GPS", "A/B Testing", "A+ Certified IT Technician",
                "A+ Certified Professional", "Ab Initio", "ABAP List Viewer (ALV)", "ABAP Web Dynpro", "Abaqus", "ABIS", "AbleCommerce",
                "Ableton", "Ableton Live", "Ableton Push", "AC3", "ACARS", "Accessibility", "Accessibility Consultancy", "Accessibility Testing",
                "Active Directory", "ActiveCampaign", "ADA", "Ada programming", "ADF / Oracle ADF", "ADO.NET", "Adobe Acrobat", "Adobe Air",
                "Adobe Analytics", "Adobe Animate", "Adobe Audition", "Adobe Captivate", "Adobe Creative Cloud", "Adobe Dynamic Tag Management",
                "Adobe Experience Manager", "Adobe Freehand", "Adobe Illustrator", "Adobe Muse", "Adobe Pagemaker", "Adobe Premiere Pro",
                "Adobe Systems", "Adobe Workfront", "Adobe XD", "Advanced Business Application Programming (ABAP)", "Affectiva", "Agile Development",
                "Agile Project Management", "Agora", "AI/RPA development", "Airtable", "AIX Administration", "AJAX", "AJAX Frameworks", "AJAX Toolkit",
                "Ajax4JSF", "Akka", "Alacra", "ALBPM", "Alexa Modification", "Algogrand", "Algolia", "Algorithm Analysis", "Alias", "Alibaba",
                "Alibre Design", "Alienbrain", "All-Source Analysis", "AlphaCAM", "Alpine JS", "Altera Quartus", "Alteryx", "Altium Designer",
                "Altium NEXUS", "Alvarion", "Amazon App Development", "Amazon CloudFormation", "Amazon CloudFront", "Amazon ECS", "Amazon FBA",
                "Amazon Listings Optimization", "Amazon Product Launch", "Amazon S3", "Amazon Web Services", "Amibroker Formula Language", "AMQP",
                "Analytics", "Anaplan", "Android App Development", "Android SDK", "Android Studio", "Android Wear SDK", "Angular", "Angular 4",
                "Angular 6", "Angular Material", "AngularJS", "Ansible", "Ansys", "AODA", "Apache", "Apache Ant", "Apache Hadoop", "Apache Kafka",
                "Apache Maven", "Apache Solr", "Apache Spark", "API", "API Development", "API Integration", "API Testing", "Apigee", "Apollo",
                "App Developer", "App Development", "App Localization", "App Publication", "App Reskin", "Appfolio", "Appian BPM", "Apple Safari",
                "Apple UIKit", "Apple Xcode", "Applescript", "Application Packaging", "Application Performance Monitoring", "Apttus", "AR / VR 3D Asset",
                "ArangoDB", "Arc", "ArcGIS", "ArchiCAD", "Architectural Engineering", "ArcMap", "ARCore", "Arena Simulation Programming",
                "Argus Monitoring Software", "Ariba", "ARKit", "Armadillo", "Articulate Storyline", "Artificial Intelligence", "AS400 & iSeries",
                "Asana", "ASM", "ASP", "ASP.NET", "ASP.NET MVC", "Aspen HYSYS", "Assembla", "Assembly", "Asterisk PBX", "Atlassian Confluence",
                "Atlassian Jira", "Atmel", "Augmented Reality", "AutoCAD Advance Steel", "AutoHotkey", "Automatic Number Plate Recognition (ANPR)",
                "Automation", "Automation Codeless Program", "AutoML", "Avaya", "AWS Amplify", "AWS Lambda", "AWS Polly", "AWS SageMaker",
                "AWS Textract", "AWS Translate", "Azure", "Backbase", "backbone.js", "Backend Development", "Background Removal", "Backtesting",
                "Baidu", "Balsamiq", "Bash", "Bash Scripting", "BeautifulSoup", "Big Data", "Big Data Sales", "BigCommerce", "BigQuery", "Binance",
                "Binance Smart Chain", "Binary Analysis", "Binary Search", "Bioinformatics", "Biostatistics", "BIRT Development", "Bitcoin", "BitMEX",
                "BitMEX API", "Bitrix", "Biztalk", "Blazor", "Blender", "Blockchain", "Blog Install", "Bluebeam", "Bluetooth Low Energy (BLE)",
                "BMC Remedy", "Boonex Dolphin", "Boost", "Bower", "Braintree", "BSD", "Bubble Developer", "BuddyPress", "Buildbox", "Bukkit",
                "Business Catalyst", "Business Central", "Business Intelligence", "C Programming", "C# Programming", "C++ Programming", "CakePHP",
                "Call Control XML", "Camio software", "Camtasia", "CAN Bus", "CapCut", "Cardano", "CARLA", "Carthage", "CasperJS", "Caspio",
                "Cassandra", "Celery", "CentOs", "Certified Ethical Hacking", "Certified Information Systems Security Professional (CISSP)",
                "Chart.js", "Charts", "Chatbot", "ChatGPT", "ChatGPT Search Optimization", "Chef Configuration Management", "Chordiant", "Chrome OS",
                "Chromium", "CI/CD", "Cinematography", "CircleCI", "CircuitMaker", "CircuitStudio", "Cisco", "Citadela", "Citrix", "ClickUp", "CLIPS",
                "Clojure", "Cloud", "Cloud Computing", "Cloud Custodian", "Cloud Data", "Cloud Development", "Cloud Finance", "Cloud Foundry",
                "Cloud Monitoring", "Cloud Networking", "Cloud Procurement", "Cloud Security", "Cloudflare", "Clover", "CMS", "CNC", "COBOL", "Cocoa",
                "Cocoa Touch", "CocoaPods", "Cocos2d", "Codeigniter", "Coding", "CoffeeScript", "Cognos", "Cold Fusion", "Color Contrast Analyzer",
                "COMPASS", "CompTIA", "Computer Graphics", "Computer Science", "Computer Security", "Computer Vision", "Construct 3", "Content Management System (CMS)",
                "Copyright", "Corda", "Cordana", "Core PHP", "Corel Draw", "Corteza", "cPanel", "CRE Loaded", "Creo", "Crestron", "Cross Browser",
                "Crowdstrike", "Cryptocurrency", "CS-Cart", "CSS2", "CSS3", "CubeCart", "CUDA", "cURL", "CV Library", "cxf", "D3.js", "Dall-E",
                "Dapper", "DApps", "Dart", "Data Backup", "Data Collection", "Data Governance", "Data Integration", "Data Management", "Data Modeling",
                "Data Modernization", "Data Visualization", "Data Warehousing", "Database Administration", "Database Development", "Database Programming",
                "DataLife Engine", "Datatables", "DDA", "DDS", "Debian", "Debugging", "Delphi", "DEMAT", "Desktop Application", "Development",
                "Development Operations", "DIALux", "Digital Marketing", "Digital Operations", "Digital Signal Processing", "Digital System Engineering",
                "DigitalOcean", "DirectX", "Discord API", "Distributed Systems", "Django", "DNS", "Docker", "Docker Compose", "Documentation", "Dogecoin",
                "Dojo", "DOM", "DOS", "DotNetNuke", "Dovecot", "Draw.io", "Dropbox API", "Drupal", "Dthreejs", "Dynamic 365", "Dynamics", "Dynatrace",
                "Dynatrace Software Monitoring", "EC Pay Workday", "Eclipse", "ECMAScript", "eCommerce", "Editorial Design", "edX", "Elasticsearch",
                "eLearning", "Electron JS", "Electronic Data Interchange (EDI)", "Electronic Forms", "Elementor", "ElevenLabs", "Elixir", "Elm",
                "Email Developer", "Embedded Software", "Ember.js", "Enterprise Architecture", "Ergo", "Erlang", "ERP Software", "ES8 Javascript",
                "Espruino", "Ethereum", "Etherscan", "ETL", "Expo", "Express JS", "Expression Engine", "Ext JS", "F#", "Face Recognition", "Facebook API",
                "Facebook Development", "Facebook Pixel", "Facebook Product Catalog", "Facebook SDK", "FastAPI", "Fastlane", "FaunaDB", "Fedora", "Figma",
                "FileMaker", "Financial Software Development", "FinTech", "Firefox", "Firewall", "Firmware", "FLANN", "Flask", "Flutter", "Formstack",
                "Forth", "Fortran", "Forum Software", "FoxyCart", "Freelancer API", "FreeSwitch", "Frontend Development", "Frontend Frameworks", "Fruugo",
                "Full Stack Development", "Funnel", "Fusion 360", "Game Consoles", "Game Design", "Game Development", "GameMaker", "GameSalad", "Gamification",
                "Garmin IQ", "GatsbyJS", "Gazebo", "GCP AI", "GenAI", "Genesis 4D", "Genetic Algebra Modelling System", "Geofencing", "Geographical Information System (GIS)",
                "GeoJSON", "GeoServer", "GIMP", "Git", "GitHub", "GitLab", "GoDaddy", "Godot", "Golang", "Google Analytics", "Google APIs", "Google App Engine",
                "Google Apps Scripts", "Google Buzz", "Google Canvas", "Google Cardboard", "Google Checkout", "Google Chrome", "Google Cloud Platform",
                "Google Cloud Storage", "Google Data Studio", "Google Docs", "Google Earth", "Google Firebase", "Google Maps API", "Google PageSpeed Insights",
                "Google Plus", "Google Search", "Google Sheets", "Google Systems", "Google Tag Management", "Google Wave", "Google Web Toolkit", "Google Webmaster Tools",
                "GoPro", "GPGPU", "GPT Vision API", "GPT-3", "GPT-4", "GPT-4V", "Gradio", "Grails", "Graphical Network Simulator-3", "Graphics Programming",
                "GraphQL", "Gravity Forms", "Graylog", "Grease Monkey", "GrooveFunnels", "Growth Hacking", "Grunt", "GTK+", "GTmetrix", "Guacamole", "Guidewire",
                "Gulp.js", "Hadoop", "Handlebars.js", "Hardware Security Module", "Haskell", "HBase", "Heroku", "Heron", "Hewlett Packard", "HeyGen", "HFT",
                "Hibernate", "Highcharts", "HIPAA", "Hive", "HomeKit", "Houdini", "HP Openview", "HP-UX", "HTC Vive", "HTML", "HTML5", "HTTP", "Hubspot", "Hugo",
                "Humanoid Robotics", "Hybrid App", "Hybris", "Hyperledger", "Hyperledger Fabric", "HyperMesh", "iBeacon", "IBM Bluemix", "IBM BPM", "IBM Cloud",
                "IBM Datapower", "IBM Integration bus", "IBM MQ", "IBM Tivoli", "IBM Tririga", "IBM Websphere Transformation Tool", "IIS", "iMacros", "IMAP",
                "Infor", "Informatica", "Informatica MDM", "Informatica Powercenter ETL", "Instagram", "Instagram API", "Internet Security", "Interspire",
                "Ionic Framework", "Ionic React", "iOS Development", "IT Operating Model", "IT Project Management", "IT strategy", "IT Transformation", "ITIL",
                "J2EE", "Jabber", "Jade Development", "Jamf", "Jamstack", "Jasmine Javascript", "Java", "Java ME", "Java Spring", "Java Technical Architecture",
                "JavaFX", "JavaScript", "Javascript ES6", "JAWS", "JD Edwards CNC", "Jenkins", "Jimdo", "Jinja2", "Jitsi", "JMeter", "Joomla", "jqGrid", "jQuery",
                "jQuery / Prototype", "JSON", "JSP", "JUCE", "Julia Development", "Julia Language", "Juniper", "JUnit", "K2", "Kajabi", "Karma Javascript",
                "Kendo UI", "Keras", "Keyboard Testing", "Keycloak", "Keyshot", "Kibana", "Kinect", "KNIME", "Knockout.js", "Kubernetes", "LabVIEW", "LAMP",
                "Laravel", "Leap Motion SDK", "LearnDash", "Learning Management Solution (LMS) Consulting", "Learning Management Systems (LMS)", "LESS/Sass/SCSS",
                "LIBSVM", "LIMS (Laboratory Information Management System)", "Linear Regression", "Link Building", "Linkedin", "LINQ", "Linux", "LinuxCNC",
                "Liquid Template", "Lisp", "Litecoin", "LiveCode", "Local Area Networking", "Lottie", "Lotus Notes", "Low Code", "Lua", "Lucee", "Lucene",
                "Lumion", "Lynx", "Mac OS", "Magento", "Magento 2", "Magic Leap", "Magnolia", "MailerLite", "Make.com", "Managed Analytics", "Map Reduce",
                "MapKit", "MariaDB", "MATLAB/Simulink", "MEAN Stack", "MERN", "MERN Stack", "Messenger Marketing", "Meta Pixel", "Metal", "Metamask", "Metatrader",
                "MetaTrader 4", "Metatrader 5", "MeteorJS", "Micropython", "Micros RES", "Microsoft", "Microsoft 365", "Microsoft Access", "Microsoft Azure",
                "Microsoft Exchange", "Microsoft Expression", "Microsoft Graph", "Microsoft Hololens", "Microsoft PowerBI", "Microsoft Project", "Microsoft SQL Server",
                "Microsoft Visio", "MicroStrategy", "Minecraft", "Mininet", "Minitab", "MMORPG", "Mobile Accessibility", "Mobile App Audit", "Mobile App Testing",
                "Mobile Development", "Modding", "MODx", "Moho", "Monday.com", "MonetDB", "MongoDB", "Monkey C", "Moodle", "MOVEit", "Moz", "MPT MosaicML", "MQL4",
                "MQL5", "MQTT", "Mule", "MuleSoft", "MVC", "MyBB", "MySpace", "MySQL", "n8n", "Nagios Core", "National Building Specification", "NAV", "Nest.js",
                "Netbeans", "Netlify", "NetSuite", "Network Administration", "Network Engineering", "Network Monitoring", "Network Security", "Next.js", "Nginx",
                "NgRx", "Ning", "NinjaTrader", "NLP", "Node.js", "Non-fungible Tokens (NFT)", "NoSQL", "NoSQL Couch & Mongo", "NotebookLM", "Notion", "NumPy",
                "Nuxt.JS", "NVDA", "OAuth", "Object Oriented Programming (OOP)", "Objective C", "OCR", "OctoberCMS", "Oculus Mobile SDK", "Oculus Rift", "Odoo",
                "Office 365", "Office Add-ins", "Offline Conversion Facebook API Integration", "OKTA", "Online Multiplayer", "Open Cart", "Open Interpreter",
                "Open Journal Systems", "Open Source", "OpenAI", "OpenBravo", "OpenBSD", "OpenCL", "OpenCV", "OpenGL", "OpenNMS", "OpenSceneGraph", "OpenSSL",
                "OpenStack", "OpenVMS", "OpenVPN", "OpenVZ", "Oracle", "Oracle Analytics", "Oracle APEX", "Oracle Database", "Oracle EBS Tech Integration",
                "Oracle Hyperion", "Oracle OBIA", "Oracle OBIEE", "Oracle Primavera", "Oracle Retail", "OSCommerce", "OTT", "Outreach.io", "P2P Network",
                "Packaging Technology", "Page Speed Optimization", "Pandas", "Papiamento", "Parallax Scrolling", "Parallel Processing", "Parallels Automation",
                "Parallels Desktop", "Pardot Development", "Pascal", "Pattern Matching", "Payment Gateway Integration", "PayPal", "PayPal API", "Paytrace",
                "PC Programming", "PCI Compliance", "PEGA PRPC", "PencilBlue CMS", "Penetration Testing", "Pentaho", "Performance Tuning", "Perl", "Phi (Microsoft)",
                "Phoenix", "PhoneGap", "Photon Multiplayer", "Photoshop Coding", "PHP", "PHP Slim", "phpBB", "phpFox", "phpMyAdmin", "PhpNuke", "PHPrunner",
                "PHPUnit", "PICK Multivalue DB", "PikaLabs", "Pine Script", "Pinterest", "Pipedrive", "PlayFab", "Playstation VR", "PLC", "Plesk", "Plivo",
                "Plugin", "Plutus", "Point of Sale", "Polarion", "Polkadot", "Polyworks Inspector", "Polyworks Software", "POP / POP3", "POS development",
                "PoseNet", "Postfix", "PostgreSQL", "PostgreSQL Programming", "Power Automate", "Power BI", "PowerApps", "Powershell", "Powtoon", "Predictive Analytics",
                "Prestashop", "Process Simulation", "Programming", "Progressive Web Apps", "Prolog", "Prometheus Monitoring", "Proto", "Protoshare", "Prototyping",
                "Protractor Javascript", "Puck.js", "Puppet", "PureScript", "Push Notification", "PyCaret", "PySpark", "Python", "Pytorch", "QlikView",
                "QR Code Making", "Qt", "Quadruped Robotics", "Quality Engineering", "Qualtrics Survey Platform", "Quarkus", "QuickBase", "Quora", "R Programming Language",
                "RabbitMQ", "Racket", "RapidWeaver", "Raspberry Pi", "Ratio Analysis", "Ray-tracing", "Razor Template Engine", "React Native", "React.js",
                "React.js Framework", "REALbasic", "Reason", "Rebranding", "Red Hat", "Redis", "Redmine", "Redshift", "Redux.js", "Regression Testing",
                "Regular Expressions", "Relux", "Replit", "REST API", "RESTful", "RESTful API", "Retrieval-Augemented Generation", "Reverse Engineering", "Revit",
                "Revit Architecture", "RichFaces", "Roadnet", "Roblox", "Robot Operating System (ROS)", "Rocket Engine", "Roslyn", "RPG Development", "RSS",
                "Ruby", "Ruby on Rails", "Rust", "RxJS", "Ryu Controller", "SaaS", "Sails.js", "Salesforce App Development", "Salesforce Commerce Cloud",
                "Salesforce Marketing Cloud", "Samsung Accessory SDK", "SAP", "SAP 4 Hana", "SAP BODS", "SAP Business Planning and Consolidation", "SAP CPI",
                "SAP HANA", "SAP Hybris", "SAP Pay", "SAP PI", "SAP Screen Personas", "SAP Transformation", "Sass", "Scala", "Scheme", "Scikit Learn", "SciPy",
                "SCORM", "Scrapy", "Screen Reader Compatibility", "Script Install", "Scripting", "Scrivener", "Scrum", "Scrum Development", "SD-WAN",
                "SDW N17 Service Qualification", "Section 508", "Segment", "Selenium", "Selenium Webdriver", "Sencha / YahooUI", "SEO", "SEO Auditing", "Server",
                "Server to Server Facebook API Integration", "ServiceNow", "SFDC", "Sharepoint", "Shell Script", "Shopify", "Shopify Development", "Shopping Cart Integration",
                "Siebel", "Silverlight", "SIP", "Sketch", "Sketching", "Slack", "Smart Contracts", "Smarty PHP", "SMTP", "Snapchat", "Snowflake", "SOAP API",
                "Social Engine", "Social Media Management", "Social Networking", "Socket IO", "Software Architecture", "Software Development", "Software Engineering",
                "Software Performance Testing", "Software Testing", "Solana", "Solaris", "Soldering", "Solidity", "Solutions Architecture", "Spark", "Sphinx",
                "Splunk", "Spring Boot", "Spring Data", "Spring JPA", "Spring Security", "SPSS Statistics", "SQL", "SQLite", "Squarespace", "Squid Cache",
                "SSIS (SQL Server Integration Services)", "SSL", "Stable Diffusion", "Steam API", "Storage Area Networks", "Storm", "Strapi", "Stripe", "Subversion",
                "SugarCRM", "SurveyMonkey", "Svelte", "SVG", "Swift", "Swift Package Manager", "Swing (Java)", "Symfony PHP", "System Admin", "System Administration",
                "System Analysis", "T-SQL (Transact Structures Query Language)", "Tableau", "TailWind", "Tailwind CSS", "TALKBACK", "Tally Definition Language",
                "TaoBao API", "Tealium", "TeamCity", "Technology Consulting", "Telegram API", "Telerik", "Tensorflow", "Teradata", "Terra", "Terraform", "Test",
                "Test Automation", "Testing / QA", "TestStand", "Tether", "Thermodynamics", "Three.js", "Tibco Spotfire", "Time & Labor SAP", "Tinkercad",
                "Titanium", "Tizen SDK for Wearables", "Toon Boom", "TopSolid", "TopSolid Wood", "TradeStation", "Travis CI", "TRON", "Troubleshooting", "Truffle",
                "Tumblr", "TvOS", "Twago", "Twilio", "Twitch", "Twitter", "Twitter API", "Typescript", "TYPO3", "Ubiquiti", "Ubuntu", "Udacity", "Umbraco",
                "UML Design", "Unbounce", "Underscore.js", "Unitree SDK Development", "Unity", "Unity 3D", "UNIX", "Unreal Engine", "Usability Testing",
                "User Experience Research", "User Interface / IA", "User Story Writing", "UX Research", "V-Play", "Vapor", "Varnish Cache", "VB.NET", "VBScript",
                "vBulletin", "Veeam", "Vercel", "Version Control Git", "Vertex AI", "VertexFX", "VideoHive", "Vim", "Virtual Machines", "Virtual Reality",
                "Virtual Worlds", "Virtuemart", "Virtuozzo", "Visual Basic", "Visual Basic for Apps", "Visual Foxpro", "Visual Studio", "Visualization", "VMware",
                "VoiceXML", "VoIP", "Volusion", "Vowpal Wabbit", "VPN", "VPS", "VSCode", "vTiger", "VtrunkD", "Vue.js", "Vue.js Framework", "Vuforia", "Vulkan",
                "Vymo", "WatchKit", "Web API", "Web Application", "Web Application Audit", "Web Content Accessibility Guidelines", "Web Crawling", "Web Design",
                "Web Development", "Web Hosting", "Web Scraping", "Web Security", "Web Services", "Web Testing", "Web3.js", "WEBDEV", "Webflow", "Weblogic",
                "webMethods", "Webpack", "WebRTC", "Website Accessibility", "Website Accessibility Remediation", "Website Analytics", "Website Audit",
                "Website Build", "Website Development", "Website Localization", "Website Management", "Website Optimization", "Website Testing", "Weebly",
                "White Hat SEO", "WHMCS", "Windchill PLM", "WINDEV", "WINDEV Mobile", "Windows 8", "Windows API", "Windows Desktop", "Windows Server",
                "Windows Service", "WinJS", "Wireguard", "Wix", "WMS", "WordPress", "WordPress Multilingual", "WordPress Plugin", "WPF", "Wrike", "Wufoo",
                "x86/x64 Assembler", "Xamarin", "XAML", "Xara", "Xcode", "Xcodebuild", "XenForo", "XHTML", "XML", "XMPP", "Xojo", "Xoops", "XPages", "xpath",
                "XQuery", "XSLT", "XSS (Cross-site scripting)", "Yandex", "Yarn", "Yii", "Yii2", "YouTube", "Zapier", "Zen Cart", "Zend", "Zendesk", "Znode",
                "Zoho", "Zoho Creator", "Zoho CRM", "Zoom"
            };

            // Writing & Content Category
            var writingContentSkills = new[]
            {
                "Abnormal Psychology", "Abstract", "Academic Medicine", "Academic Publishing", "Academic Research", "Academic Writing", "Annuals",
                "Apple iBooks Author", "Article Rewriting", "Article Writing", "Beta Reading", "Biography Writing", "Blog", "Blog Writing", "Blogging",
                "Book Review", "Book Writing", "Braille", "Business Plan Writing", "Business Writing", "Cartography & Maps", "Case Study Writing",
                "Catch Phrases", "Comedy Writing", "Communications", "Compliance and Safety Procedures Writer", "Content Audit", "Content Creation",
                "Content Development", "Content Strategy", "Content Writing", "Copy Editing", "Copy Typing", "Copywriting", "Cover Letter", "Creative Writing",
                "Domain Research", "eBooks", "Editing", "Editorial Writing", "Educational Research", "English Translation", "Environmental Science",
                "Essay Writing", "Fact Checking", "Fashion Writing", "Fiction", "Financial Research", "Forum Posting", "Ghostwriting", "Grant Writing",
                "Headlines", "Investigative Journalism", "Journalism", "LaTeX", "Legal Writing", "LinkedIn Profile", "Manuscripts", "Medical Research",
                "Medical Writing", "Memoir Writing", "Newsletters", "Non-Fiction Writing", "Online Writing", "PDF", "Pitch Deck Writing", "Podcast Writing",
                "Poetry", "Powerpoint", "Press Releases", "Product Descriptions", "Proofreading", "Proposal Writing", "Publishing", "Report Writing",
                "Research", "Research Writing", "Resumes", "Reviews", "RFP Writing", "Romance Writing", "Scientific Writing", "Screenwriting", "Script Writing",
                "SEO Writing", "Short Stories", "Slogans", "Social Media Copy", "Speech Writing", "Survey Research", "Taglines", "Technical Documentation",
                "Technical Writing", "Test Plan Writing", "Test Strategy Writing", "Translation", "Travel Writing", "Web Page Writer", "White Paper",
                "WIKI", "Wikipedia", "Word Processing", "Writing"
            };

            // Design & Media Category
            var designMediaSkills = new[]
            {
                "2D Animation", "2D Animation Explainer Video", "2D Drafting", "2D Drawing", "2D Game Art", "2D Layout", "360-degree video", "3D Animation",
                "3D Architecture", "3D CAD", "3D Design", "3D Drafting", "3D Layout", "3D Logo", "3D Model Maker", "3D Modelling", "3D Rendering", "3D Rigging",
                "3D Scanning", "3D Studio Max", "3D Visualization", "3ds Max", "A/V design", "A/V editing", "A/V Engineering", "A/V Systems", "A&R", "Acting",
                "ActionScript", "Adobe Creative Suite", "Adobe Dreamweaver", "Adobe Fireworks", "Adobe Flash", "Adobe FrameMaker", "Adobe InDesign",
                "Adobe Lightroom", "Adobe LiveCycle Designer", "Adobe Photoshop", "Adobe Robohelp", "Advertisement Design", "Affinity Designer", "Affinity Photo",
                "After Effects", "AI Rendering", "Album Design", "Album Production", "Alternative Rock", "Alto Flute", "Android UI Design", "Animated Video Development",
                "Animation", "Animoto", "App Design", "Apple Compressor", "Apple Logic Pro", "Apple Motion", "Architectural Rendering", "Architectural Visualization",
                "Architecture", "Art Consulting", "Artist & Repertoire Administration", "Arts & Crafts", "Audio Ads", "Audio Editing", "Audio Engineering",
                "Audio Mastering", "Audio Production", "Audio Services", "Audiobook", "Audiobook Narration", "AutoCAD Architecture", "Autodesk", "Autodesk Civil 3D",
                "Autodesk Fusion 360", "Autodesk Inventor", "Autodesk Revit", "Autodesk Sketchbook Pro", "Avid", "Axure", "Banner Design", "Beautiful AI",
                "Blog Design", "Book Artist", "Book Cover Design", "Book Design", "Bootstrap", "BricsCAD", "Brochure Design", "Building Architecture",
                "Building Information Modeling", "Building Regulations", "Business Card Design", "Calligraphy", "Canva", "Capture NX2", "Card Design",
                "Caricature & Cartoons", "Catalog Design", "Cel Animation", "CGI", "Character Illustration", "Childrens Book Illustration", "Cinema 4D",
                "Clip Studio Paint", "CMS IntelliCAD", "Collage Making", "Color Grading", "Comics", "Commercials", "Concept Art", "Concept Design", "Corel Painter",
                "CorelDRAW", "Corporate Identity", "Costume Design", "Covers & Packaging", "Creative Design", "CSS", "Cutout Animation", "CV Design", "DaVinci Resolve",
                "Design", "Design Optimization", "Design Thinking", "Digital Art", "Digital Cinema Packages", "Digital Painting", "Digital Product Design",
                "Doodle", "Draftsight", "Drawing Artist", "eBook Design", "eLearning Designer", "Evernote", "Explainer Videos", "Fabric Printing Design",
                "Facade Design", "Fashion Consulting", "Fashion Design", "Fashion Modeling", "Film Production", "Filmmaking", "Final Cut Pro", "Final Cut Pro X",
                "Finale / Sibelius", "Fire Alarm Design", "FL Studio", "Flash 3D", "Flash Animation", "Flex", "Floor Plan", "Flow Charts", "FlowVella",
                "Flyer Design", "Format and Layout", "Framer", "Front-end Design", "Furniture Design", "Game Art", "Game Trailer", "Game UI", "GarageBand",
                "Generative Design", "Genially", "GIF", "GIF Animation", "Google Slides", "GraffixPro Studio", "Graphic Art", "Graphic Design", "GstarCAD",
                "Haiku Deck", "Handy Sketch Pad", "Icon Design", "Illustration", "Illustrator", "Image Consultation", "Image Processing", "iMovie",
                "Industrial Design", "Infographics", "Infrastructure Architecture", "Inkscape", "Instructional Design", "Interaction Design", "Interior Design",
                "Intros & Outros", "Invision", "Invitation Design", "Isometric Animation", "JDF", "Jingles & Intros", "Keynote", "Kinetic Typography", "Kizoa",
                "Krita", "Label Design", "Landing Pages", "Lettering", "Level Design", "Lighting Design", "Logo Animation", "Logo Design", "Magazine Design",
                "Make Real", "Makerbot", "Manga", "Matte Painting", "Maya", "Mentimeter", "Menu Design", "Microservices", "MIDI", "Mood Board", "Motion Design",
                "Motion Graphics", "Music", "Music Management", "Music Production", "Music Transcription", "Music Video", "NanoCAD", "Neo4j", "NX CAD",
                "Oil Painting", "Package Design", "Packaging Design", "Pattern Design", "Pattern Making", "Performing Arts", "Photo Editing", "Photo Restoration",
                "Photo Retouching", "Photography", "Photoshop", "Photoshop Design", "Pixel Art", "Podcast Editing", "Post-Production", "Poster Design",
                "Pre-production", "Pre-production Animation", "Precast Designer", "Presentations", "Prezi", "Print", "Print Design", "Procreate", "Product Cover",
                "Product Photography", "ProgeCAD", "Prototype Design", "PSD to HTML", "PSD2CMS", "QuarkXPress", "Radio Announcement", "Research Drone Footages",
                "Resin", "Rhino 3D", "Rotoscoping", "RWD", "Seamless Printing", "Shopify Templates", "Sign Design", "SketchUp", "Slidebean", "SmartDraw",
                "Social Media Post Design", "SolidEdge", "Sound Design", "Sound Effects", "Sound Engineering", "SoundCloud", "Sports Design", "Stationery Design",
                "Sticker Design", "Storyboard", "T-Shirts", "Tattoo Design", "Technical Drawing / Tech Pack", "Tekla Structures", "Templates", "Textile Design",
                "TikZ", "Tldraw", "Town Planning", "Traditional Animation", "Twitter Spaces", "Typography", "UI / User Interface", "Unigraphics NX", "Urban Design",
                "User Ergonomics", "UX / User Experience", "V-Ray", "Vector Design", "Vector Tracing", "Vectorization", "Vectorworks", "Vehicle Signage",
                "VFX Art", "Video Ads", "Video Broadcasting", "Video Editing", "Video Post-editing", "Video Production", "Video Services", "Video Streaming",
                "Video Tours", "Videography", "VideoScribe", "Vimeo", "Virtual Staging", "Visme", "Visual Arts", "Visual Design", "Visual Effects", "Voice Acting",
                "Voice Artist", "Voice Over", "Voice Talent", "Watercolor Painting", "Web Animation", "Website Design", "Whiteboard", "Whiteboard Animation",
                "Wireframes", "Word", "WordPress Design", "Yahoo! Store Design", "YouTube Video Editing", "Zbrush", "Zoho Show"
            };

            // Data Entry & Admin Category
            var dataEntryAdminSkills = new[]
            {
                "ABBYY FineReader", "Academic Administration", "ANOVA", "Answering Telephones", "Article Submission", "Bookkeeping", "BPO", "Call Center",
                "Chat Operation", "Contact Center Services", "Customer Service", "Customer Support", "Data Analytics", "Data Annotating", "Data Architecture",
                "Data Cleansing", "Data Delivery", "Data Entry", "Data Extraction", "Data Processing", "Data Scraping", "Database Design", "Desktop Support",
                "Email Handling", "ePub", "Excel", "Excel Macros", "Excel VB Capabilities", "Excel VBA", "General Office", "Google Spreadsheets", "GPT Agent",
                "Helpdesk", "Infographic and Powerpoint Slide Designing", "Investment Research", "LibreOffice", "Microsoft Office", "Microsoft Outlook",
                "Microsoft Word", "Order Processing", "Phone Support", "PostgreSQL Administration", "Procurement", "Qlik", "Qualitative Research", "qwerty",
                "Records Management", "Relational Databases", "SAP Master Data Governance", "Software Documentation", "Spreadsheets", "Technical Support",
                "Telegram Moderation", "Telephone Handling", "Time Management", "Transcription", "Typeform", "Typing", "Video Upload", "Web Search"
            };

            // Translation & Languages Category
            var translationLanguagesSkills = new[]
            {
                "Afar", "Afrikaans Translator", "Albanian Translator", "American Sign Language Translator", "Amharic", "Arabic Translator", "Armenian Translator",
                "Assamese", "Basque Translator", "Bengali Translator", "Bosnian Translator", "Breton Translator", "Bulgarian Translator", "Burmese Translator",
                "Canadian French Translator", "Castilian Spanish Translator", "Catalan Translator", "Croatian Translator", "Czech Translator", "Danish Translator",
                "Dari Translator", "Dinka Translator", "Dutch Translator", "English (UK) Translator", "English (US) Translator", "English Grammar", "English Spelling",
                "Estonian Translator", "Filipino Translator", "Finnish Translator", "French Translator", "Game Translation", "Georgian Translator", "German Translator",
                "Greek Translator", "Gujrati Translator", "Hebrew Translator", "Hindi Translator", "Hungarian Translator", "Icelandic Translator", "Indonesian Translator",
                "Interpreter", "Irish Translator", "Italian Translator", "Japanese Translator", "Kannada Translator", "Karelian Translator", "Kazakh Translator",
                "Korean Translator", "Kurdish Translator", "Latin Translator", "Latvian Translator", "Linguistics", "Lithuanian Translator", "Macedonian Translator",
                "Malay Translator", "Malayalam Translator", "Maltese Translator", "Marathi Translator", "Montenegrin Translator", "Nepali Translator", "Norwegian Translator",
                "Oromo", "Pashto Translator", "Poet", "Polish Translator", "Portuguese (Brazil) Translator", "Portuguese Translator", "Punjabi Translator",
                "Romanian Translator", "Russian Translator", "Sami Translator", "Serbian Translator", "Serbo-Croatian Translator", "Simplified Chinese Translator",
                "Sinhalese Translator", "Slovakian Translator", "Slovenian Translator", "Somali", "Spanish Translator", "Subtitles & Captions", "Swahili Translator",
                "Swedish Translator", "Tajik Translator", "Tamil Translator", "Technical Translation", "Telugu Translator", "Thai Translator", "Tigrinya",
                "Traditional Chinese (Hong Kong)", "Traditional Chinese (Taiwan)", "Turkish Translator", "Ukrainian Translator", "Urdu Translator", "Vietnamese Translator",
                "Welsh Translator", "Yiddish Translator", "Yoruba Translator"
            };

            // Sales & Marketing Category
            var salesMarketingSkills = new[]
            {
                "ABR Accredited Buyer Representative", "ABR Designation", "Ad Planning & Buying", "Advertising", "Affiliate Marketing", "Agency Relationship Management",
                "Airbnb", "Aircraft Sales", "Amazon Ads", "Analytics Sales", "ATS Sales", "B2B Marketing", "Basecamp", "Bing Ads", "Book Marketing", "Brand Management",
                "Brand Marketing", "Branding", "Bulk Marketing", "Channel Account Management", "Channel Sales", "Classifieds Posting", "ClickBank", "ClickFunnels",
                "Cloud Sales", "Competitor Analysis", "Content Marketing", "Conversion Rate Optimization", "CRM", "Crowdfunding", "Customer Retention Marketing",
                "Datacenter Sales", "Digital Agency Sales", "Digital Strategy", "Direct Mail", "Drip", "eBay", "Email Campaign", "Email Marketing", "Emerging Accounts",
                "Enterprise Sales", "Enterprise Sales Management", "Etsy", "Eventbrite", "Facebook Ads", "Facebook Marketing", "Facebook Shops", "Facebook Verification",
                "Field Sales", "Field Sales Management", "Financial Sales", "Google Ads", "Google Adsense", "Google Adwords", "Google Shopping", "Healthcare Sales",
                "HootSuite", "HR Sales", "Hubspot Marketing", "IDM Sales", "Inbound Marketing", "Indiegogo", "Influencer Marketing", "Inside Sales", "Instagram Ads",
                "Instagram Marketing", "Instagram Verification", "Interactive Advertising", "Internet Marketing", "Internet Research", "ISV Sales", "Kartra", "Keap",
                "Keyword Research", "Kickstarter", "Klaviyo", "Lead Generation", "Leads", "Life Science Sales", "Mailchimp", "Mailwizz", "Market Analysis",
                "Market Research", "Marketing", "Marketing Strategy", "Marketo", "Media Relations", "Media Sales", "Medical Devices Sales", "MLM", "Mobile Sales",
                "Multi Level Marketing", "Network Sales", "OEM Account Management", "OEM Sales", "Pardot Marketing", "Payroll Sales", "Periscope", "Podcast Marketing",
                "Podcasting", "PPC Marketing", "Product Marketing", "Recruiting Sales", "Reseller", "Retail Sales", "SaaS Sales", "Sales", "Sales Account Management",
                "Sales Management", "Sales Promotion", "Search Engine Marketing", "Security Sales", "SEMrush", "SendGrid", "SEOMoz", "Social Media Marketing",
                "Social Sales", "Social Video Marketing", "Software Sales", "Soundcloud Promotion", "Spotify Ads", "Sprout", "Technology Sales", "Telecom Sales",
                "Telemarketing", "TikTok", "Tiktok Ads", "Twitter Marketing", "Unboxing Videos", "User Research", "Viral Marketing", "Visual Merchandising",
                "WooCommerce", "YouTube Ads"
            };

            // Add all skills to the list with their respective categories
            foreach (var skill in websitesItSoftwareSkills)
            {
                userSkills.Add(new UserSkill { Id = Guid.NewGuid(), Category = "Websites, IT & Software", Name = skill });
            }

            foreach (var skill in writingContentSkills)
            {
                userSkills.Add(new UserSkill { Id = Guid.NewGuid(), Category = "Writing & Content", Name = skill });
            }

            foreach (var skill in designMediaSkills)
            {
                userSkills.Add(new UserSkill { Id = Guid.NewGuid(), Category = "Design & Media", Name = skill });
            }

            foreach (var skill in dataEntryAdminSkills)
            {
                userSkills.Add(new UserSkill { Id = Guid.NewGuid(), Category = "Data Entry & Admin", Name = skill });
            }

            foreach (var skill in translationLanguagesSkills)
            {
                userSkills.Add(new UserSkill { Id = Guid.NewGuid(), Category = "Translation & Languages", Name = skill });
            }

            foreach (var skill in salesMarketingSkills)
            {
                userSkills.Add(new UserSkill { Id = Guid.NewGuid(), Category = "Sales & Marketing", Name = skill });
            }

            // Add all skills to the database
            await context.UserSkills.AddRangeAsync(userSkills);
            await context.SaveChangesAsync();
        }
    }
}