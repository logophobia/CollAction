﻿using CollAction.Services.User.Models;
using GraphQL.Types;

namespace CollAction.GraphQl.Mutations
{
    public class UserResultGraph : ObjectGraphType<UserResult>
    {
        public UserResultGraph()
        {
            Field(x => x.User, true);
            Field(x => x.Result);
        }
    }
}
