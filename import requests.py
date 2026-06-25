import requests
import re

def decode_secret_message(doc_url):
    response = requests.get(doc_url)
    response.raise_for_status()
    content = response.text
    
    pattern = r'(\d+)\s+([^\s]+)\s+(\d+)'
    matches = re.findall(pattern, content)
    
    if not matches:
        print("No data found in the document.")
        return
    
    points = []
    max_x = 0
    max_y = 0
    
    for x_str, char, y_str in matches:
        x = int(x_str)
        y = int(y_str)
        points.append((x, y, char))
        max_x = max(max_x, x)
        max_y = max(max_y, y)
    
    grid = [[' ' for _ in range(max_x + 1)] for _ in range(max_y + 1)]
    
    for x, y, char in points:
        grid[y][x] = char
    
    for row in grid:
        print(''.join(row))

decode_secret_message("https://docs.google.com/document/d/e/2PACX-1vSvM5gDlNvt7npYHhp_XfsJvuntUhq184By5xO_pA4b_gCWeXb6dM6ZxwN8rE6S4ghUsCj2VKR21oEP/pub")