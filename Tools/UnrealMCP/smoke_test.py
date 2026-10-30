"""Exercise real MCP stdio initialization, discovery and read-only calls."""
import asyncio
import json
from pathlib import Path
import sys
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

async def main():
    root = Path(__file__).resolve().parents[2]
    params = StdioServerParameters(command=sys.executable, args=[str(Path(__file__).with_name('server.py'))])
    async with stdio_client(params) as streams:
        async with ClientSession(*streams) as session:
            await session.initialize()
            tools = await session.list_tools()
            names = [t.name for t in tools.tools]
            assert len(names) == 6, names
            status = await session.call_tool('rishi_status', {})
            assert not status.isError, status
            canon = await session.call_tool('rishi_canon', {'mission_id':0})
            assert not canon.isError
            payload=json.loads(canon.content[0].text)
            assert len(payload['missions']) == 8
            assert payload['missions'][0]['Title']=='Navami ki Raat'
            assert payload['missions'][-1]['Title']=='Damodar ke Neeche'
            bad = await session.call_tool('rishi_canon', {'mission_id':9})
            assert bad.isError
            editor = await session.call_tool('rishi_list_actors', {})
            print(json.dumps({'tools':names,'status':json.loads(status.content[0].text),'missions':len(payload['missions']),'invalid_mission_rejected':True,'live_actor_inspection_ok':not editor.isError},indent=2))

if __name__=='__main__': asyncio.run(main())
