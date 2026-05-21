import os
import re
from urllib.parse import urlparse

# --- Configuration ---
SITEMAP_FILE = 'sitemap.xml'
ADS_TXT_CONTENT = "google.com, pub-2533086861741403, DIRECT, f08c47fec0942fa0"

ADS_SNIPPET = """
<!-- =========== Google Ads Scripts And Meta Tags [START] ============-->
<script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-2533086861741403"
     crossorigin="anonymous"></script>
<meta name="google-adsense-account" content="ca-pub-2533086861741403">
<!-- =========== Google Ads Scripts And Meta Tags [END] ============-->
"""

def create_ads_txt():
    """Creates the ads.txt file in the root directory."""
    with open('ads.txt', 'w', encoding='utf-8') as f:
        f.write(ADS_TXT_CONTENT)
    print("✅ Created ads.txt successfully.")

def process_sitemap():
    """Reads the sitemap, extracts .html URLs, and injects the script."""
    if not os.path.exists(SITEMAP_FILE):
        print(f"❌ Error: Could not find '{SITEMAP_FILE}' in the current directory.")
        return

    # Read the sitemap file
    with open(SITEMAP_FILE, 'r', encoding='utf-8') as f:
        sitemap_content = f.read()

    # Extract all URLs using regex to bypass potential XML namespace issues
    urls = re.findall(r'<loc>(.*?)</loc>', sitemap_content)
    
    html_files_processed = 0
    html_files_skipped = 0
    
    for url in urls:
        if url.endswith('.html'):
            # Convert the URL path to a local file path
            # e.g., https://yoursite.com/folder/page.html -> folder/page.html
            parsed_url = urlparse(url)
            local_path = parsed_url.path.lstrip('/')
            
            if local_path:
                status = inject_code(local_path)
                if status == 'injected':
                    html_files_processed += 1
                elif status == 'skipped':
                    html_files_skipped += 1
                    
    print(f"\n📊 Summary: Injected code into {html_files_processed} files. Skipped {html_files_skipped} files (already had code).")

def inject_code(file_path):
    """Injects the Google Ads snippet into the specific HTML file before </head>."""
    if not os.path.exists(file_path):
        print(f"⚠️ Warning: File '{file_path}' is in the sitemap but missing from local folder.")
        return 'missing'

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception as e:
        print(f"❌ Error reading '{file_path}': {e}")
        return 'error'

    # Idempotency check: Don't inject if the script is already there
    if "ca-pub-2533086861741403" in content:
        print(f"⏭️ Skipped: '{file_path}' (Google Ads code already present)")
        return 'skipped'

    # Use regex to find </head> (case-insensitive) and inject the snippet before it
    # \1 represents the matched closing head tag so we don't accidentally delete it
    new_content, num_replacements = re.subn(
        r'(</head>)', 
        f'{ADS_SNIPPET}\\1', 
        content, 
        flags=re.IGNORECASE
    )

    if num_replacements > 0:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"✅ Injected: '{file_path}'")
        return 'injected'
    else:
        print(f"⚠️ Warning: Could not find </head> tag in '{file_path}'")
        return 'no_head_tag'

if __name__ == "__main__":
    print("Starting Google Ads Injection Script...\n")
    create_ads_txt()
    process_sitemap()