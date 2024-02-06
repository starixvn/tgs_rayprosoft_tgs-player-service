using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tgs.PlayerService;

public interface IPlayerLoginRecordRepository
{
    IAsyncEnumerable<string> QueryGroupPlayerAsync(Guid brandId,DateTimeOffset startOn, DateTimeOffset endOn, int offset = 0, int limit = 100, CancellationToken cancellationToken = default);
}
