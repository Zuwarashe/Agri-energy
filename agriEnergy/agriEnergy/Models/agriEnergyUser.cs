﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace agriEnergy.Models;

// Add profile data for application users by adding properties to the agriEnergyUser class
public class agriEnergyUser : IdentityUser
{//--------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// gets first name of user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
    [PersonalData]
    [Column(TypeName = "nvarchar(MAX)")]
    public string firstName { get; set; }
}

//---------------------------------------- END OF FILE -------------------------------------------------------//
